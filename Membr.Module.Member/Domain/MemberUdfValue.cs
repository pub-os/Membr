namespace Membr.Module.Member.Domain;

public class MemberUdfValue
{
    public int Id { get; set; }
    public int MemberId { get; set; }
    public Member Member { get; set; } = null!;
    public int UdfDefinitionId { get; set; }
    public UdfDefinition UdfDefinition { get; set; } = null!;
    public string? Value { get; set; }
}
