namespace Membr.Module.Identity;

public class JwtOptions
{
    public const string SectionName = "Auth:Standalone:Jwt";

    public required string Issuer { get; set; }
    public required string Audience { get; set; }
    public required string SigningKey { get; set; }
    public int AccessTokenMinutes { get; set; } = 15;
    public int RefreshTokenDays { get; set; } = 14;
}
