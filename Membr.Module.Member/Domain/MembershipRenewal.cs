namespace Membr.Module.Member.Domain;

public class MembershipRenewal
{
    public int Id { get; set; }

    public int MembershipId { get; set; }
    public Membership Membership { get; set; } = null!;

    public DateTime RenewedAt { get; set; }
    public DateTime PreviousEndDate { get; set; }
    public DateTime NewEndDate { get; set; }
}
