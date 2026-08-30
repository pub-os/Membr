namespace Membr.Module.Member.Application.Handlers.Memberships;

using Microsoft.EntityFrameworkCore;
using Persistence;

internal sealed class ListMemberMembershipsHandler(MembersDbContext db, TimeProvider clock)
{
    public async Task<List<MembershipDto>> Handle(int memberId, CancellationToken ct)
    {
        var now = clock.GetUtcNow().UtcDateTime;

        var memberships = await db.Memberships
            .Include(m => m.MembershipType)
            .Where(m => m.MemberId == memberId)
            .OrderByDescending(m => m.StartDate)
            .ToListAsync(ct);

        return [.. memberships.Select(m => MembershipDto.FromEntity(m, now))];
    }
}
