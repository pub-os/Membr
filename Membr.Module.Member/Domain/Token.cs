using Membr.Shared.Domain;

namespace Membr.Module.Member.Domain;

public class Token : EntityBase
{
    public int Id { get; set; }
    public int MemberId { get; set; }
    public Member? Member { get; set; }
    public TokenType TokenType { get; set; }
    public string Value { get; set; } = null!;
    public bool IsRevoked { get; set; }
    public DateTime? RevokedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public enum TokenType
{
    Rfid,
    Qr,
    Barcode,
    Number,
}
