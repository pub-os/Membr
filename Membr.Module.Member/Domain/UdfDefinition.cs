namespace Membr.Module.Member.Domain;

public class UdfDefinition
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public UdfFieldType Type { get; set; }
    public bool IsActive { get; set; } = true;

    // Only meaningful when Type == MultiSelect.
    public List<string> Options { get; set; } = [];

    // Canonical serialized form matching MemberUdfValue.Value: "true"/"false" for Bool,
    // ISO date/datetime for Date/DateTime, plain text for String, JSON array for MultiSelect.
    public string? DefaultValue { get; set; }
}
