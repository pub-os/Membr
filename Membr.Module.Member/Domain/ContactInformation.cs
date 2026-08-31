using Membr.Shared.Domain;

namespace Membr.Module.Member.Domain;

public class ContactInformation : EntityBase
{
    public int Id { get; set; }
    public int MemberId { get; set; }
    public Member? Member { get; set; }
    public ContactType ContactType { get; set; }
    public string ContactDetail { get; set; } = null!;
    public bool IsPrimary { get; set; }
}

public enum ContactType
{
    Email,
    Phone,
}
