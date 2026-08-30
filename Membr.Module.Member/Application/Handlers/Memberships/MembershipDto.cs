namespace Membr.Module.Member.Application.Handlers.Memberships;

using Domain;

internal sealed record MembershipDto(
    int Id,
    int MemberId,
    int MembershipTypeId,
    string MembershipTypeName,
    DateTime StartDate,
    DateTime EndDate,
    bool IsActive)
{
    public static MembershipDto FromEntity(Membership m, DateTime utcNow) => new(
        m.Id,
        m.MemberId,
        m.MembershipTypeId,
        m.MembershipType.Name,
        m.StartDate,
        m.EndDate,
        m.IsActive(utcNow));
}
