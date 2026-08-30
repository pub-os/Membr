using Membr.Module.Identity.Persistence;

namespace Membr.Module.Identity.Application.Handlers;

using Domain;
using Microsoft.EntityFrameworkCore;

internal sealed class RefreshHandler(
    IdentityDbContext db,
    TokenService tokenService,
    TimeProvider clock)
{
    public async Task<RefreshResult> Handle(RefreshRequest request, CancellationToken ct)
    {
        var incomingHash = TokenService.Hash(request.RefreshToken);

        var existing = await db.RefreshTokens
            .Include(t => t.User)
            .SingleOrDefaultAsync(t => t.TokenHash == incomingHash, ct);

        if (existing is null)
            return RefreshResult.Failed();

        var now = clock.GetUtcNow().UtcDateTime;

        if (existing.RevokedAtUtc is not null)
        {
            // This token was already used/revoked once but is being presented again —
            // that means it either leaked or a client is replaying a stale token.
            // Treat it as compromise: kill every active token for the user so a stolen
            // token chain can't keep refreshing.
            var activeTokens = await db.RefreshTokens
                .Where(t => t.UserId == existing.UserId && t.RevokedAtUtc == null)
                .ToListAsync(ct);

            foreach (var token in activeTokens)
                token.RevokedAtUtc = now;

            await db.SaveChangesAsync(ct);
            return RefreshResult.Failed();
        }

        if (now >= existing.ExpiresAtUtc)
            return RefreshResult.Failed();

        var accessToken = await tokenService.CreateAccessToken(existing.User);
        var (rawRefreshToken, hash, expiresAtUtc) = tokenService.CreateRefreshToken();

        var replacement = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = existing.UserId,
            TokenHash = hash,
            ExpiresAtUtc = expiresAtUtc,
            CreatedAtUtc = now
        };

        existing.RevokedAtUtc = now;
        existing.ReplacedByTokenId = replacement.Id;

        db.RefreshTokens.Add(replacement);
        await db.SaveChangesAsync(ct);

        return RefreshResult.Ok(accessToken, rawRefreshToken);
    }
}

internal sealed record RefreshRequest(string RefreshToken);

internal sealed class RefreshResult
{
    public bool Succeeded { get; private init; }
    public string? AccessToken { get; private init; }
    public string? RefreshToken { get; private init; }

    public static RefreshResult Ok(string accessToken, string refreshToken) =>
        new() { Succeeded = true, AccessToken = accessToken, RefreshToken = refreshToken };

    public static RefreshResult Failed() => new() { Succeeded = false };
}
