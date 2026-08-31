using Membr.Module.Member.Application.Handlers;
using Membr.Module.Member.Application.Handlers.Contacts;
using Membr.Module.Member.Application.Handlers.Dashboard;
using Membr.Module.Member.Application.Handlers.Members;
using Membr.Module.Member.Application.Handlers.Memberships;
using Membr.Module.Member.Application.Handlers.MembershipTypes;
using Membr.Module.Member.Application.Handlers.Settings;
using Membr.Module.Member.Application.Handlers.Tokens;
using Membr.Module.Member.Application.Handlers.Udf;
using Membr.Module.Member.Endpoints.Admin;
using Membr.Module.Member.Endpoints.External;
using Membr.Module.Member.Persistence;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Membr.Module.Member;

// Membr.Modules.Members/MembersModule.cs  ← the ONLY public type

public static class MemberModule
{
    public static IServiceCollection AddMembersModule(
        this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<MembersDbContext>(o => o
            .UseNpgsql(config.GetConnectionString("Default"),
                b => b.MigrationsHistoryTable("__EFMigrationsHistory", "members"))
            .UseSnakeCaseNamingConvention());

        services.AddScoped<CreateMemberHandler>();
        services.AddScoped<GetMemberHandler>();
        services.AddScoped<SearchMemberHandler>();
        services.AddScoped<ListMemberHandler>();
        services.AddScoped<CreateMembershipTypeHandler>();
        services.AddScoped<ListMembershipTypeHandler>();
        services.AddScoped<GetMembershipTypeHandler>();
        services.AddScoped<CreateMembershipHandler>();
        services.AddScoped<RenewMembershipHandler>();
        services.AddScoped<ListMemberMembershipsHandler>();
        services.AddScoped<GetMembershipSettingsHandler>();
        services.AddScoped<UpdateMembershipSettingsHandler>();
        services.AddScoped<GetDashboardStatsHandler>();
        services.AddScoped<CreateUdfDefinitionHandler>();
        services.AddScoped<ListUdfDefinitionsHandler>();
        services.AddScoped<GetUdfDefinitionHandler>();
        services.AddScoped<UpdateUdfDefinitionHandler>();
        services.AddScoped<DeleteUdfDefinitionHandler>();
        services.AddScoped<ApplyDefaultToAllMembersHandler>();
        services.AddScoped<ListMemberUdfValuesHandler>();
        services.AddScoped<ListAllMemberUdfValuesHandler>();
        services.AddScoped<UpdateMemberUdfValueHandler>();
        services.AddScoped<ListMemberContactsHandler>();
        services.AddScoped<CreateMemberContactHandler>();
        services.AddScoped<UpdateMemberContactHandler>();
        services.AddScoped<DeleteMemberContactHandler>();
        services.AddScoped<CreateMemberTokenHandler>();
        services.AddScoped<ListMemberTokensHandler>();
        services.AddScoped<RevokeMemberTokenHandler>();
        services.AddScoped<LookupTokenHandler>();
        return services;
    }

    public static void MapMembersEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapMemberAdminRoutes();
        app.MapMemberExternalRoutes();
    }

    public static async Task MigrateAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MembersDbContext>();
        await db.Database.MigrateAsync();
    }

    public static Task SeedMembersAsync(IServiceProvider services, int count) =>
        MemberSeeder.SeedAsync(services, count);
}
