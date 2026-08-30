namespace Membr.Module.Member.Domain;

public enum MembershipRenewalMode
{
    Rolling,
    FixedTerm,
}

public class MembershipType
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public bool IsActive { get; set; }

    public MembershipRenewalMode RenewalMode { get; set; }

    // Rolling: how long a membership lasts from whenever it starts/renews.
    public int? DurationMonths { get; set; }

    // FixedTerm: the calendar day (month/day) every membership of this type expires on.
    public int? FixedTermAnchorMonth { get; set; }
    public int? FixedTermAnchorDay { get; set; }
}
