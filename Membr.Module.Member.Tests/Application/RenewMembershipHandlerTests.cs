using Membr.Module.Member.Application.Handlers.Memberships;
using Membr.Module.Member.Domain;
using Microsoft.Extensions.Time.Testing;

namespace Membr.Module.Member.Tests.Application;

public class RenewMembershipHandlerTests
{
    private static Membr.Module.Member.Domain.Member NewMember() => new() { FirstName = "Ada", Surname = "Lovelace", DateOfBirth = new DateOnly(1990, 1, 1) };

    [Fact]
    public async Task Rolling_RenewWhileActive_ExtendsFromCurrentEndDate()
    {
        await using var db = TestDbContextFactory.Create();
        var member = NewMember();
        var type = new MembershipType { Name = "Standard", IsActive = true, RenewalMode = MembershipRenewalMode.Rolling, DurationMonths = 1 };
        db.Members.Add(member);
        db.MembershipTypes.Add(type);
        var membership = new Membr.Module.Member.Domain.Membership
        {
            MemberId = member.Id,
            MembershipTypeId = type.Id,
            MembershipType = type,
            StartDate = new DateTime(2026, 2, 15, 0, 0, 0, DateTimeKind.Utc),
            EndDate = new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc),
        };
        db.Memberships.Add(membership);
        await db.SaveChangesAsync(CancellationToken.None);

        var clock = new FakeTimeProvider(new DateTimeOffset(2026, 3, 15, 0, 0, 0, TimeSpan.Zero));
        var handler = new RenewMembershipHandler(db, clock);

        var result = await handler.Handle(member.Id, membership.Id, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(membership.Id, result!.Id);
        Assert.Equal(member.Id, result.MemberId);
        Assert.Equal(new DateTime(2026, 5, 1, 0, 0, 0, DateTimeKind.Utc), result.EndDate);
    }

    [Fact]
    public async Task FixedTerm_RenewAfterExpiry_JumpsToNextAnchor()
    {
        await using var db = TestDbContextFactory.Create();
        var member = NewMember();
        var type = new MembershipType
        {
            Name = "Annual",
            IsActive = true,
            RenewalMode = MembershipRenewalMode.FixedTerm,
            FixedTermAnchorMonth = 1,
            FixedTermAnchorDay = 1,
        };
        db.Members.Add(member);
        db.MembershipTypes.Add(type);
        var membership = new Membr.Module.Member.Domain.Membership
        {
            MemberId = member.Id,
            MembershipTypeId = type.Id,
            MembershipType = type,
            StartDate = new DateTime(2025, 5, 1, 0, 0, 0, DateTimeKind.Utc),
            EndDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        };
        db.Memberships.Add(membership);
        await db.SaveChangesAsync(CancellationToken.None);

        var clock = new FakeTimeProvider(new DateTimeOffset(2026, 5, 1, 0, 0, 0, TimeSpan.Zero));
        var handler = new RenewMembershipHandler(db, clock);

        var result = await handler.Handle(member.Id, membership.Id, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(new DateTime(2027, 1, 1, 0, 0, 0, DateTimeKind.Utc), result!.EndDate);
        Assert.True(result.IsActive);
    }

    [Fact]
    public async Task ReturnsNull_WhenMembershipBelongsToADifferentMember()
    {
        await using var db = TestDbContextFactory.Create();
        var member = NewMember();
        var otherMember = NewMember();
        var type = new MembershipType { Name = "Standard", IsActive = true, RenewalMode = MembershipRenewalMode.Rolling, DurationMonths = 12 };
        db.Members.AddRange(member, otherMember);
        db.MembershipTypes.Add(type);
        var membership = new Membr.Module.Member.Domain.Membership
        {
            MemberId = member.Id,
            MembershipTypeId = type.Id,
            MembershipType = type,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddYears(1),
        };
        db.Memberships.Add(membership);
        await db.SaveChangesAsync(CancellationToken.None);

        var handler = new RenewMembershipHandler(db, new FakeTimeProvider());

        var result = await handler.Handle(otherMember.Id, membership.Id, CancellationToken.None);

        Assert.Null(result);
    }
}
