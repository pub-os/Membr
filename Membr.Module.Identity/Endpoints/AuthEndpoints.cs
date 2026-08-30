namespace Membr.Module.Identity.Endpoints;

using System.Security.Claims;
using Application.Handlers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;

internal static class AuthEndpoints
{
    private const string RefreshTokenCookie = "refreshToken";

    public static void MapAuthRoutes(this IEndpointRouteBuilder app)
    {
        var auth = app.MapGroup("/auth")
            .WithTags("Auth");

        auth.MapPost("/login", Login)
            .WithName("Login")
            .WithSummary("Sign in with email and password")
            .RequireRateLimiting(IdentityModule.AuthRateLimitPolicy);

        auth.MapPost("/refresh", Refresh)
            .WithName("Refresh")
            .WithSummary("Exchange a refresh token for a new access token")
            .RequireRateLimiting(IdentityModule.AuthRateLimitPolicy);

        auth.MapPost("/logout", Logout)
            .WithName("Logout")
            .WithSummary("Revoke the current refresh token")
            .RequireAuthorization();

        auth.MapGet("/me", Me)
            .WithName("Me")
            .WithSummary("Get the current authenticated user")
            .RequireAuthorization();
    }

    private static async Task<Results<Ok<AccessTokenResponse>, UnauthorizedHttpResult>> Login(
        LoginRequest request,
        LoginHandler handler,
        HttpContext http,
        IOptions<JwtOptions> jwtOptions,
        CancellationToken ct)
    {
        var result = await handler.Handle(request, ct);
        if (!result.Succeeded)
            return TypedResults.Unauthorized();

        SetRefreshTokenCookie(http, result.RefreshToken!, jwtOptions.Value);
        return TypedResults.Ok(new AccessTokenResponse(result.AccessToken!));
    }

    private static async Task<Results<Ok<AccessTokenResponse>, UnauthorizedHttpResult>> Refresh(
        RefreshHandler handler,
        HttpContext http,
        IOptions<JwtOptions> jwtOptions,
        CancellationToken ct)
    {
        if (!http.Request.Cookies.TryGetValue(RefreshTokenCookie, out var refreshToken) ||
            string.IsNullOrEmpty(refreshToken))
            return TypedResults.Unauthorized();

        var result = await handler.Handle(new RefreshRequest(refreshToken), ct);
        if (!result.Succeeded)
        {
            DeleteRefreshTokenCookie(http);
            return TypedResults.Unauthorized();
        }

        SetRefreshTokenCookie(http, result.RefreshToken!, jwtOptions.Value);
        return TypedResults.Ok(new AccessTokenResponse(result.AccessToken!));
    }

    private static async Task<NoContent> Logout(
        LogoutHandler handler,
        HttpContext http,
        CancellationToken ct)
    {
        if (http.Request.Cookies.TryGetValue(RefreshTokenCookie, out var refreshToken) &&
            !string.IsNullOrEmpty(refreshToken))
        {
            await handler.Handle(new LogoutRequest(refreshToken), ct);
        }

        DeleteRefreshTokenCookie(http);
        return TypedResults.NoContent();
    }

    private static Ok<MeResponse> Me(ClaimsPrincipal user)
    {
        var id = user.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var email = user.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
        var displayName = user.FindFirstValue(ClaimTypes.Name) ?? string.Empty;
        var roles = user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray();

        return TypedResults.Ok(new MeResponse(id, email, displayName, roles));
    }

    private static void SetRefreshTokenCookie(HttpContext http, string refreshToken, JwtOptions jwtOptions)
    {
        http.Response.Cookies.Append(RefreshTokenCookie, refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = http.Request.IsHttps,
            SameSite = SameSiteMode.Strict,
            Path = "/auth",
            Expires = DateTimeOffset.UtcNow.AddDays(jwtOptions.RefreshTokenDays)
        });
    }

    private static void DeleteRefreshTokenCookie(HttpContext http) =>
        http.Response.Cookies.Delete(RefreshTokenCookie, new CookieOptions { Path = "/auth" });
}

internal sealed record AccessTokenResponse(string AccessToken);

internal sealed record MeResponse(string Id, string Email, string DisplayName, string[] Roles);
