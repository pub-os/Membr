using Membr.Module.Member.Application.Handlers.Tokens;
using Membr.Module.Member.Domain;
using Microsoft.Extensions.Time.Testing;

namespace Membr.Module.Member.Tests.Application.Tokens;

public class CreateMemberTokenHandlerTests
{
    private static Membr.Module.Member.Domain.Member NewMember() => new() { FirstName = "Ada", Surname = "Lovelace", DateOfBirth = new DateOnly(1990, 1, 1) };

    [Fact]
    public async Task CreatesToken_ForExistingMember()
    {
        await using var db = TestDbContextFactory.Create();
        var member = NewMember();
        db.Members.Add(member);
        await db.SaveChangesAsync(CancellationToken.None);

        var clock = new FakeTimeProvider(new DateTimeOffset(2026, 3, 15, 0, 0, 0, TimeSpan.Zero));
        var handler = new CreateMemberTokenHandler(db, clock);

        var result = await handler.Handle(member.Id, new CreateMemberTokenRequest(TokenType.Rfid, "ABC123"), CancellationToken.None);

        Assert.Equal(CreateMemberTokenStatus.Success, result.Status);
        Assert.Equal("ABC123", result.Token!.Value);
        Assert.False(result.Token.IsRevoked);
    }

    [Fact]
    public async Task ReturnsMemberNotFound_WhenMemberDoesNotExist()
    {
        await using var db = TestDbContextFactory.Create();
        var clock = new FakeTimeProvider(DateTimeOffset.UtcNow);
        var handler = new CreateMemberTokenHandler(db, clock);

        var result = await handler.Handle(999, new CreateMemberTokenRequest(TokenType.Rfid, "ABC123"), CancellationToken.None);

        Assert.Equal(CreateMemberTokenStatus.MemberNotFound, result.Status);
    }

    [Fact]
    public async Task RejectsDuplicateValue_WhenAlreadyAssignedToAnActiveToken()
    {
        await using var db = TestDbContextFactory.Create();
        var member1 = NewMember();
        var member2 = NewMember();
        db.Members.AddRange(member1, member2);
        await db.SaveChangesAsync(CancellationToken.None);

        var clock = new FakeTimeProvider(DateTimeOffset.UtcNow);
        var handler = new CreateMemberTokenHandler(db, clock);

        var first = await handler.Handle(member1.Id, new CreateMemberTokenRequest(TokenType.Rfid, "ABC123"), CancellationToken.None);
        var second = await handler.Handle(member2.Id, new CreateMemberTokenRequest(TokenType.Rfid, "ABC123"), CancellationToken.None);

        Assert.Equal(CreateMemberTokenStatus.Success, first.Status);
        Assert.Equal(CreateMemberTokenStatus.Invalid, second.Status);
    }

    [Fact]
    public async Task AllowsReissuingValue_OnceOriginalTokenIsRevoked()
    {
        await using var db = TestDbContextFactory.Create();
        var member1 = NewMember();
        var member2 = NewMember();
        db.Members.AddRange(member1, member2);
        await db.SaveChangesAsync(CancellationToken.None);

        var clock = new FakeTimeProvider(DateTimeOffset.UtcNow);
        var createHandler = new CreateMemberTokenHandler(db, clock);
        var revokeHandler = new RevokeMemberTokenHandler(db, clock);

        var first = await createHandler.Handle(member1.Id, new CreateMemberTokenRequest(TokenType.Rfid, "ABC123"), CancellationToken.None);
        await revokeHandler.Handle(member1.Id, first.Token!.Id, CancellationToken.None);

        var second = await createHandler.Handle(member2.Id, new CreateMemberTokenRequest(TokenType.Rfid, "ABC123"), CancellationToken.None);

        Assert.Equal(CreateMemberTokenStatus.Success, second.Status);
    }
}
