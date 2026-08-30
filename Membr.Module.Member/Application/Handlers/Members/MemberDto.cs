namespace Membr.Module.Member.Application.Handlers.Members;

using Domain;
internal sealed record MemberDto(int Id, string FirstName, string Surname, DateOnly DateOfBirth)
{
    public static MemberDto FromEntity(Member m) => new(m.Id, m.FirstName, m.Surname, m.DateOfBirth);
}
