namespace Membr.Module.Member.Application.Handlers.Tokens;

using Domain;

internal sealed record TokenLookupMembershipDto(
    string MembershipTypeName,
    DateTime StartDate,
    DateTime EndDate,
    bool IsActive)
{
    public static TokenLookupMembershipDto FromEntity(Membership m, DateTime utcNow) => new(
        m.MembershipType.Name, m.StartDate, m.EndDate, m.IsActive(utcNow));
}

internal sealed record TokenLookupDto(
    int MemberId,
    string FirstName,
    string Surname,
    List<TokenLookupMembershipDto> Memberships);
