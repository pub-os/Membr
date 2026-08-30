namespace Membr.Module.Member.Application.Handlers.Members;

using Persistence;

internal sealed class GetMemberHandler(MembersDbContext db)
{
    public async Task<MemberDto?> Handle(GetMemberQuery query, CancellationToken ct)
    {
        var member = await db.Members.FindAsync([query.Id], ct);
        if (member == null)
            return null;
        return new MemberDto(member.Id, member.FirstName, member.Surname, member.DateOfBirth);
    }
}

internal sealed record GetMemberQuery(int Id);
