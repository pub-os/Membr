namespace Membr.Module.Member.Domain;

/// <summary>
/// Singleton (single-row) table of global admin-configurable membership settings.
/// Add further global settings here as fields, not new tables.
/// </summary>
public class MembershipSettings
{
    public const int SingletonId = 1;

    public int Id { get; set; } = SingletonId;
    public bool AllowMultipleMemberships { get; set; }
}
