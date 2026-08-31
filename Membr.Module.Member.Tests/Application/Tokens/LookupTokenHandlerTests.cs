using Membr.Module.Member.Application.Handlers.Tokens;
using Membr.Module.Member.Domain;
using Microsoft.Extensions.Time.Testing;

namespace Membr.Module.Member.Tests.Application.Tokens;

public class LookupTokenHandlerTests
{
    private static Membr.Module.Member.Domain.Member NewMember() => new() { FirstName = "Ada", Surname = "Lovelace", DateOfBirth = new DateOnly(1990, 1, 1) };

    private static MembershipType RollingType() => new()
    {
        Name = "Standard",
        IsActive = true,
        RenewalMode = MembershipRenewalMode.Rolling,
        DurationMonths = 12,
    };

    [Fact]
    public async Task ReturnsMemberAndMemberships_ForActiveToken()
    {
        await using var db = TestDbContextFactory.Create();
        var member = NewMember();
        var type = RollingType();
        db.Members.Add(member);
        db.MembershipTypes.Add(type);
        await db.SaveChangesAsync(CancellationToken.None);

        var clock = new FakeTimeProvider(new DateTimeOffset(2026, 3, 15, 0, 0, 0, TimeSpan.Zero));
        db.Memberships.Add(new Membership
        {
            MemberId = member.Id,
            MembershipTypeId = type.Id,
            StartDate = clock.GetUtcNow().UtcDateTime,
            EndDate = clock.GetUtcNow().UtcDateTime.AddYears(1),
        });
        db.Tokens.Add(new Token { MemberId = member.Id, TokenType = TokenType.Rfid, Value = "ABC123", CreatedAt = clock.GetUtcNow().UtcDateTime });
        await db.SaveChangesAsync(CancellationToken.None);

        var handler = new LookupTokenHandler(db, clock);
        var result = await handler.Handle("ABC123", CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(member.Id, result!.MemberId);
        Assert.Single(result.Memberships);
        Assert.True(result.Memberships[0].IsActive);
    }

    [Fact]
    public async Task ReturnsNull_ForRevokedToken()
    {
        await using var db = TestDbContextFactory.Create();
        var member = NewMember();
        db.Members.Add(member);
        await db.SaveChangesAsync(CancellationToken.None);

        var clock = new FakeTimeProvider(DateTimeOffset.UtcNow);
        db.Tokens.Add(new Token
        {
            MemberId = member.Id,
            TokenType = TokenType.Rfid,
            Value = "ABC123",
            CreatedAt = clock.GetUtcNow().UtcDateTime,
            IsRevoked = true,
            RevokedAt = clock.GetUtcNow().UtcDateTime,
        });
        await db.SaveChangesAsync(CancellationToken.None);

        var handler = new LookupTokenHandler(db, clock);
        var result = await handler.Handle("ABC123", CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task ReturnsNull_ForUnknownValue()
    {
        await using var db = TestDbContextFactory.Create();
        var clock = new FakeTimeProvider(DateTimeOffset.UtcNow);
        var handler = new LookupTokenHandler(db, clock);

        var result = await handler.Handle("does-not-exist", CancellationToken.None);

        Assert.Null(result);
    }
}
