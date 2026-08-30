using Membr.Module.Member.Persistence;

namespace Membr.Module.Member.Application.Handlers.MembershipTypes;

internal sealed class GetMembershipTypeHandler(MembersDbContext db)
{
    public async Task<MembershipTypeDto?> Handle(GetMembershipTypeQuery query, CancellationToken ct)
    {
        var type = await db.MembershipTypes.FindAsync([query.Id], ct);
        if (type == null)
            return null;
        return MembershipTypeDto.FromEntity(type);
    }
}
internal sealed record GetMembershipTypeQuery(int Id);
