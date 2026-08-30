using Membr.Shared.Domain;

namespace Membr.Module.Member.Domain;

public class ContactInformation : EntityBase
{
    public int Id { get; set; }
    public ContactType ContactType { get; set; }
    public string ContactDetail { get; set; } = null!;
}

public enum ContactType
{
    Email,
    Phone,
}
public enum ContactPriority
{
    Primary,
}
