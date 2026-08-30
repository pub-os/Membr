namespace Membr.Module.Member.Application.Handlers.MembershipTypes;

using Domain;

internal sealed record MembershipTypeDto(
    int Id,
    string Name,
    string? Description,
    bool IsActive,
    MembershipRenewalMode RenewalMode,
    int? DurationMonths,
    int? FixedTermAnchorMonth,
    int? FixedTermAnchorDay)
{
    public static MembershipTypeDto FromEntity(MembershipType mt) => new(
        mt.Id,
        mt.Name,
        mt.Description,
        mt.IsActive,
        mt.RenewalMode,
        mt.DurationMonths,
        mt.FixedTermAnchorMonth,
        mt.FixedTermAnchorDay);
}
