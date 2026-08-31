using System.Text;
using Membr.Module.Identity.Application;
using Membr.Module.Identity.Auth;
using Membr.Module.Identity.Application.Handlers;
using Membr.Module.Identity.Application.Handlers.Users;
using Membr.Module.Identity.Domain;
using Membr.Module.Identity.Endpoints;
using Membr.Module.Identity.Endpoints.Admin;
using Membr.Module.Identity.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Threading.RateLimiting;

namespace Membr.Module.Identity;

public static class IdentityModule
{
    public const string AuthRateLimitPolicy = "auth";

    public static IServiceCollection AddIdentityModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<IdentityDbContext>(o => o
            .UseNpgsql(configuration.GetConnectionString("Default"),
                b => b.MigrationsHistoryTable("__EFMigrationsHistory", "identity"))
            .UseSnakeCaseNamingConvention());

        services.AddIdentityCore<ApplicationUser>(options =>
        {
            options.Password.RequiredLength = 6;
            options.Password.RequireNonAlphanumeric = false;
            options.User.RequireUniqueEmail = true;
        })
        .AddRoles<ApplicationRole>()
        .AddEntityFrameworkStores<IdentityDbContext>()
        .AddSignInManager()
        .AddDefaultTokenProviders();

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        var jwt = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
            ?? throw new ArgumentNullException(JwtOptions.SectionName);
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwt.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwt.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30)
                };
            })
            .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>(ApiKeyAuthenticationHandler.SchemeName, _ => { });

        services.AddAuthorizationBuilder()
            .AddPolicy("AdminOnly", p => p.RequireRole("Admin"))
            .AddPolicy("DeviceAccess", p => p
                .AddAuthenticationSchemes(ApiKeyAuthenticationHandler.SchemeName)
                .RequireAuthenticatedUser());

        // Throttles credential-guessing against /auth/login and /auth/refresh: UserManager.CheckPasswordAsync
        // doesn't go through SignInManager, so Identity's own lockout counter is never incremented and this
        // is the only brute-force protection those endpoints have.
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddPolicy(AuthRateLimitPolicy, httpContext =>
                RateLimitPartition.GetSlidingWindowLimiter(
                    partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    factory: _ => new SlidingWindowRateLimiterOptions
                    {
                        PermitLimit = 5,
                        Window = TimeSpan.FromMinutes(5),
                        SegmentsPerWindow = 5,
                        QueueLimit = 0
                    }));
        });

        services.AddScoped<TokenService>();
        services.AddScoped<LoginHandler>();
        services.AddScoped<RefreshHandler>();
        services.AddScoped<LogoutHandler>();
        services.AddScoped<ListUsersHandler>();
        services.AddScoped<CreateUserHandler>();
        services.AddScoped<ListRolesHandler>();
        return services;
    }

    public static void MapIdentityEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapAuthRoutes();
        app.MapAdminUserRoutes();
    }

    public static async Task MigrateAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var provider = scope.ServiceProvider;

        var db = provider.GetRequiredService<IdentityDbContext>();
        await db.Database.MigrateAsync();

        var roleManager = provider.GetRequiredService<RoleManager<ApplicationRole>>();
        foreach (var roleName in new[] { "Admin", "User" })
        {
            if (!await roleManager.RoleExistsAsync(roleName))
                await roleManager.CreateAsync(new ApplicationRole { Name = roleName, Enabled = true });
        }

        var config = provider.GetRequiredService<IConfiguration>();
        var adminEmail = config["Auth:Standalone:InitialAdmin:Email"];
        var adminPassword = config["Auth:Standalone:InitialAdmin:Password"];
        if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
            return;

        var userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();
        if (await userManager.FindByEmailAsync(adminEmail) is not null)
            return;

        var admin = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            DisplayName = "Administrator",
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(admin, adminPassword);
        if (result.Succeeded)
            await userManager.AddToRoleAsync(admin, "Admin");
    }
}
