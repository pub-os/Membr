using Membr.Module.Identity.Persistence;

namespace Membr.Module.Identity.Application.Handlers;


using Domain;
using Microsoft.AspNetCore.Identity;

internal sealed class LoginHandler(
    UserManager<ApplicationUser> userManager,
    TokenService tokenService,
    IdentityDbContext db,
    TimeProvider clock)
{
    public async Task<LoginResult> Handle(LoginRequest request, CancellationToken ct)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null)
            return LoginResult.Failed();

        var passwordOk = await userManager.CheckPasswordAsync(user, request.Password);
        if (!passwordOk)
            return LoginResult.Failed();

        var accessToken = await tokenService.CreateAccessToken(user);
        var (rawRefreshToken, hash, expiresAtUtc) = tokenService.CreateRefreshToken();

        db.RefreshTokens.Add(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = hash,
            ExpiresAtUtc = expiresAtUtc,
            CreatedAtUtc = clock.GetUtcNow().UtcDateTime
        });
        await db.SaveChangesAsync(ct);

        return LoginResult.Ok(accessToken, rawRefreshToken);
    }
}

internal sealed record LoginRequest(string Email, string Password);

internal sealed class LoginResult
{
    public bool Succeeded { get; private init; }
    public string? AccessToken { get; private init; }
    public string? RefreshToken { get; private init; }

    public static LoginResult Ok(string accessToken, string refreshToken) =>
        new() { Succeeded = true, AccessToken = accessToken, RefreshToken = refreshToken };

    public static LoginResult Failed() => new() { Succeeded = false };
}
