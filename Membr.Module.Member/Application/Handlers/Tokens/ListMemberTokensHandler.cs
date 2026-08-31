namespace Membr.Module.Member.Application.Handlers.Tokens;

using Microsoft.EntityFrameworkCore;
using Persistence;

internal sealed class ListMemberTokensHandler(MembersDbContext db)
{
    public async Task<List<TokenDto>> Handle(int memberId, CancellationToken ct)
    {
        var tokens = await db.Tokens
            .Where(t => t.MemberId == memberId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(ct);

        return [.. tokens.Select(TokenDto.FromEntity)];
    }
}
