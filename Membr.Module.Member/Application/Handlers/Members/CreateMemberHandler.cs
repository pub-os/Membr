namespace Membr.Module.Member.Application.Handlers.Members;

using Domain;
using Persistence;

internal sealed class CreateMemberHandler(MembersDbContext db, TimeProvider clock)
{
    public async Task<MemberDto> Handle(CreateMemberRequest command, CancellationToken ct)
    {
        var member = new Member()
        {
            FirstName = command.FirstName,
            Surname = command.Surname,
            DateOfBirth = command.DateOfBirth
        };

        db.Members.Add(member);
        await db.SaveChangesAsync(ct);

        return new MemberDto(member.Id, member.FirstName, member.Surname, member.DateOfBirth);
    }
}

internal sealed record CreateMemberRequest(string FirstName, string Surname, DateOnly DateOfBirth);
