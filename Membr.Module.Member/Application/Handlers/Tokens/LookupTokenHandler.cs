namespace Membr.Module.Member.Application.Handlers.Tokens;

using Microsoft.EntityFrameworkCore;
using Persistence;

internal sealed class LookupTokenHandler(MembersDbContext db, TimeProvider clock)
{
    public async Task<TokenLookupDto?> Handle(string value, CancellationToken ct)
    {
        var token = await db.Tokens
            .Include(t => t.Member)
            .FirstOrDefaultAsync(t => t.Value == value && !t.IsRevoked, ct);

        if (token?.Member is null)
            return null;

        var now = clock.GetUtcNow().UtcDateTime;

        var memberships = await db.Memberships
            .Include(m => m.MembershipType)
            .Where(m => m.MemberId == token.MemberId)
            .ToListAsync(ct);

        return new TokenLookupDto(
            token.Member.Id,
            token.Member.FirstName,
            token.Member.Surname,
            [.. memberships.Select(m => TokenLookupMembershipDto.FromEntity(m, now))]);
    }
}
