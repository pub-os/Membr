using Membr.Module.Member.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Membr.Module.Member.Application.Handlers.MembershipTypes;

internal sealed class ListMembershipTypeHandler(MembersDbContext db)
{
    public async Task<List<MembershipTypeDto>> Handle(CancellationToken ct)
    {
        var membershipTypes = await db.MembershipTypes.ToListAsync(ct);
        return [.. membershipTypes.Select(MembershipTypeDto.FromEntity)];
    }
}
