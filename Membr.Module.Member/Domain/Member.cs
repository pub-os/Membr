using Membr.Shared.Domain;

namespace Membr.Module.Member.Domain;

public class Member : EntityBase
{
    public int Id { get; set; }
    public string FirstName { get; set; } = null!;
    public string Surname { get; set; } = null!;
    public DateOnly DateOfBirth { get; set; }
}
