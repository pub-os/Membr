namespace Membr.Module.Member.Application.Handlers.Tokens;

using Domain;
using Microsoft.EntityFrameworkCore;
using Persistence;

internal sealed class CreateMemberTokenHandler(MembersDbContext db, TimeProvider clock)
{
    public async Task<CreateMemberTokenResult> Handle(int memberId, CreateMemberTokenRequest request, CancellationToken ct)
    {
        var memberExists = await db.Members.AnyAsync(m => m.Id == memberId, ct);
        if (!memberExists)
            return CreateMemberTokenResult.MemberNotFound();

        if (string.IsNullOrWhiteSpace(request.Value))
            return CreateMemberTokenResult.Invalid("Token value is required.");

        var valueInUse = await db.Tokens.AnyAsync(t => t.Value == request.Value && !t.IsRevoked, ct);
        if (valueInUse)
            return CreateMemberTokenResult.Invalid("This token value is already assigned to a member.");

        var token = new Token
        {
            MemberId = memberId,
            TokenType = request.TokenType,
            Value = request.Value,
            CreatedAt = clock.GetUtcNow().UtcDateTime,
        };

        db.Tokens.Add(token);
        await db.SaveChangesAsync(ct);

        return CreateMemberTokenResult.Success(TokenDto.FromEntity(token));
    }
}

internal enum CreateMemberTokenStatus
{
    Success,
    MemberNotFound,
    Invalid,
}

internal sealed record CreateMemberTokenResult(CreateMemberTokenStatus Status, TokenDto? Token, string? Error)
{
    public static CreateMemberTokenResult Success(TokenDto dto) => new(CreateMemberTokenStatus.Success, dto, null);
    public static CreateMemberTokenResult MemberNotFound() => new(CreateMemberTokenStatus.MemberNotFound, null, null);
    public static CreateMemberTokenResult Invalid(string error) => new(CreateMemberTokenStatus.Invalid, null, error);
}

internal sealed record CreateMemberTokenRequest(TokenType TokenType, string Value);
