namespace Membr.Module.Member.Application.Handlers.Tokens;

using Microsoft.EntityFrameworkCore;
using Persistence;

internal sealed class RevokeMemberTokenHandler(MembersDbContext db, TimeProvider clock)
{
    public async Task<bool> Handle(int memberId, int tokenId, CancellationToken ct)
    {
        var token = await db.Tokens.FirstOrDefaultAsync(t => t.Id == tokenId && t.MemberId == memberId, ct);
        if (token is null || token.IsRevoked)
            return false;

        token.IsRevoked = true;
        token.RevokedAt = clock.GetUtcNow().UtcDateTime;
        await db.SaveChangesAsync(ct);
        return true;
    }
}
