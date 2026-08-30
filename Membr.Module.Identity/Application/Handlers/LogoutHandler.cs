using Membr.Module.Identity.Persistence;

namespace Membr.Module.Identity.Application.Handlers;

using Microsoft.EntityFrameworkCore;

internal sealed class LogoutHandler(IdentityDbContext db, TimeProvider clock)
{
    public async Task Handle(LogoutRequest request, CancellationToken ct)
    {
        var incomingHash = TokenService.Hash(request.RefreshToken);

        var existing = await db.RefreshTokens
            .SingleOrDefaultAsync(t => t.TokenHash == incomingHash, ct);

        if (existing is null || existing.RevokedAtUtc is not null)
            return;

        existing.RevokedAtUtc = clock.GetUtcNow().UtcDateTime;
        await db.SaveChangesAsync(ct);
    }
}

internal sealed record LogoutRequest(string RefreshToken);
