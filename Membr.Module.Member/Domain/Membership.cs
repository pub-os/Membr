namespace Membr.Module.Member.Domain;

public class Membership
{
    public int Id { get; set; }

    public int MemberId { get; set; }
    public Member Member { get; set; } = null!;

    public int MembershipTypeId { get; set; }
    public MembershipType MembershipType { get; set; } = null!;

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public bool IsActive(DateTime utcNow) => EndDate >= utcNow;
}
