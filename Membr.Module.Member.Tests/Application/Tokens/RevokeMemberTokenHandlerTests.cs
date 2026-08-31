using Membr.Module.Member.Application.Handlers.Tokens;
using Membr.Module.Member.Domain;
using Microsoft.Extensions.Time.Testing;

namespace Membr.Module.Member.Tests.Application.Tokens;

public class RevokeMemberTokenHandlerTests
{
    private static Membr.Module.Member.Domain.Member NewMember() => new() { FirstName = "Ada", Surname = "Lovelace", DateOfBirth = new DateOnly(1990, 1, 1) };

    [Fact]
    public async Task RevokesToken_WhenItExistsForMember()
    {
        await using var db = TestDbContextFactory.Create();
        var member = NewMember();
        db.Members.Add(member);
        await db.SaveChangesAsync(CancellationToken.None);

        var clock = new FakeTimeProvider(new DateTimeOffset(2026, 3, 15, 0, 0, 0, TimeSpan.Zero));
        var token = new Token { MemberId = member.Id, TokenType = TokenType.Rfid, Value = "ABC123", CreatedAt = clock.GetUtcNow().UtcDateTime };
        db.Tokens.Add(token);
        await db.SaveChangesAsync(CancellationToken.None);

        var handler = new RevokeMemberTokenHandler(db, clock);
        var revoked = await handler.Handle(member.Id, token.Id, CancellationToken.None);

        Assert.True(revoked);
        Assert.True(token.IsRevoked);
        Assert.Equal(clock.GetUtcNow().UtcDateTime, token.RevokedAt);
    }

    [Fact]
    public async Task ReturnsFalse_WhenTokenDoesNotBelongToMember()
    {
        await using var db = TestDbContextFactory.Create();
        var member = NewMember();
        var otherMember = NewMember();
        db.Members.AddRange(member, otherMember);
        await db.SaveChangesAsync(CancellationToken.None);

        var clock = new FakeTimeProvider(DateTimeOffset.UtcNow);
        var token = new Token { MemberId = member.Id, TokenType = TokenType.Rfid, Value = "ABC123", CreatedAt = clock.GetUtcNow().UtcDateTime };
        db.Tokens.Add(token);
        await db.SaveChangesAsync(CancellationToken.None);

        var handler = new RevokeMemberTokenHandler(db, clock);
        var revoked = await handler.Handle(otherMember.Id, token.Id, CancellationToken.None);

        Assert.False(revoked);
    }

    [Fact]
    public async Task ReturnsFalse_WhenTokenAlreadyRevoked()
    {
        await using var db = TestDbContextFactory.Create();
        var member = NewMember();
        db.Members.Add(member);
        await db.SaveChangesAsync(CancellationToken.None);

        var clock = new FakeTimeProvider(DateTimeOffset.UtcNow);
        var token = new Token
        {
            MemberId = member.Id,
            TokenType = TokenType.Rfid,
            Value = "ABC123",
            CreatedAt = clock.GetUtcNow().UtcDateTime,
            IsRevoked = true,
            RevokedAt = clock.GetUtcNow().UtcDateTime,
        };
        db.Tokens.Add(token);
        await db.SaveChangesAsync(CancellationToken.None);

        var handler = new RevokeMemberTokenHandler(db, clock);
        var revoked = await handler.Handle(member.Id, token.Id, CancellationToken.None);

        Assert.False(revoked);
    }
}
