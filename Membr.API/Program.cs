using System.Text.Json.Serialization;
using Membr.Module.Identity;
using Membr.Module.Member;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddMembersModule(builder.Configuration);
builder.Services.AddIdentityModule(builder.Configuration);
builder.Services.ConfigureHttpJsonOptions(o => o.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

const string webCorsPolicy = "Web";
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? ["http://localhost:4200", "https://localhost:4200"];

builder.Services.AddCors(options =>
{
    options.AddPolicy(webCorsPolicy, policy => policy
        .WithOrigins(allowedOrigins)
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials());
});

var app = builder.Build();

if (args.Contains("seed"))
{
    await MemberModule.MigrateAsync(app.Services);
    var seedCount = 2000;
    var countArgIndex = Array.IndexOf(args, "seed") + 1;
    if (countArgIndex < args.Length && int.TryParse(args[countArgIndex], out var parsedCount))
        seedCount = parsedCount;

    await MemberModule.SeedMembersAsync(app.Services, seedCount);
    Console.WriteLine($"Seeded {seedCount} members.");
    return;
}

await MemberModule.MigrateAsync(app.Services);
await IdentityModule.MigrateAsync(app.Services);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
    app.UseDefaultFiles();
    app.UseStaticFiles();
}

app.UseCors(webCorsPolicy);

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

var _ = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapMembersEndpoints();
app.MapIdentityEndpoints();

if (!app.Environment.IsDevelopment())
{
    // Angular SPA — any route that isn't an API route or a static asset falls back to index.html.
    app.MapFallbackToFile("index.html");
}

app.Run();

