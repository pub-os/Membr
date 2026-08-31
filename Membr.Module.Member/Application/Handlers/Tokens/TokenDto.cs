namespace Membr.Module.Member.Application.Handlers.Tokens;

using Domain;

internal sealed record TokenDto(
    int Id,
    int MemberId,
    TokenType TokenType,
    string Value,
    bool IsRevoked,
    DateTime? RevokedAt,
    DateTime CreatedAt)
{
    public static TokenDto FromEntity(Token t) => new(
        t.Id, t.MemberId, t.TokenType, t.Value, t.IsRevoked, t.RevokedAt, t.CreatedAt);
}
