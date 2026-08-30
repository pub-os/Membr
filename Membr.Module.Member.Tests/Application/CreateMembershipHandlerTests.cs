using Membr.Module.Member.Application.Handlers.Memberships;
using Membr.Module.Member.Domain;
using Microsoft.Extensions.Time.Testing;

namespace Membr.Module.Member.Tests.Application;

public class CreateMembershipHandlerTests
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
    public async Task CreatesRollingMembership_WithEndDateOneDurationAway()
    {
        await using var db = TestDbContextFactory.Create();
        var member = NewMember();
        var type = RollingType();
        db.Members.Add(member);
        db.MembershipTypes.Add(type);
        await db.SaveChangesAsync(CancellationToken.None);

        var clock = new FakeTimeProvider(new DateTimeOffset(2026, 3, 15, 0, 0, 0, TimeSpan.Zero));
        var handler = new CreateMembershipHandler(db, clock);

        var result = await handler.Handle(member.Id, new CreateMembershipRequest(type.Id), CancellationToken.None);

        Assert.Equal(CreateMembershipStatus.Success, result.Status);
        Assert.Equal(new DateTime(2027, 3, 15, 0, 0, 0, DateTimeKind.Utc), result.Membership!.EndDate);
    }

    [Fact]
    public async Task RejectsSecondActiveMembership_WhenMultipleMembershipsNotAllowed()
    {
        await using var db = TestDbContextFactory.Create();
        var member = NewMember();
        var type = RollingType();
        db.Members.Add(member);
        db.MembershipTypes.Add(type);
        await db.SaveChangesAsync(CancellationToken.None);

        db.MembershipSettings.Add(new MembershipSettings { AllowMultipleMemberships = false });
        await db.SaveChangesAsync(CancellationToken.None);

        var clock = new FakeTimeProvider(new DateTimeOffset(2026, 3, 15, 0, 0, 0, TimeSpan.Zero));
        var handler = new CreateMembershipHandler(db, clock);

        var first = await handler.Handle(member.Id, new CreateMembershipRequest(type.Id), CancellationToken.None);
        var second = await handler.Handle(member.Id, new CreateMembershipRequest(type.Id), CancellationToken.None);

        Assert.Equal(CreateMembershipStatus.Success, first.Status);
        Assert.Equal(CreateMembershipStatus.MultipleMembershipsNotAllowed, second.Status);
    }

    [Fact]
    public async Task AllowsSecondActiveMembership_WhenMultipleMembershipsAllowed()
    {
        await using var db = TestDbContextFactory.Create();
        var member = NewMember();
        var type = RollingType();
        db.Members.Add(member);
        db.MembershipTypes.Add(type);
        await db.SaveChangesAsync(CancellationToken.None);

        db.MembershipSettings.Add(new MembershipSettings { AllowMultipleMemberships = true });
        await db.SaveChangesAsync(CancellationToken.None);

        var clock = new FakeTimeProvider(new DateTimeOffset(2026, 3, 15, 0, 0, 0, TimeSpan.Zero));
        var handler = new CreateMembershipHandler(db, clock);

        var first = await handler.Handle(member.Id, new CreateMembershipRequest(type.Id), CancellationToken.None);
        var second = await handler.Handle(member.Id, new CreateMembershipRequest(type.Id), CancellationToken.None);

        Assert.Equal(CreateMembershipStatus.Success, first.Status);
        Assert.Equal(CreateMembershipStatus.Success, second.Status);
    }

    [Fact]
    public async Task AllowsNewMembership_WhenPreviousOneHasExpired_EvenIfMultipleNotAllowed()
    {
        await using var db = TestDbContextFactory.Create();
        var member = NewMember();
        var type = RollingType();
        db.Members.Add(member);
        db.MembershipTypes.Add(type);
        db.Memberships.Add(new Membr.Module.Member.Domain.Membership
        {
            MemberId = member.Id,
            MembershipTypeId = type.Id,
            StartDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            EndDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        });
        await db.SaveChangesAsync(CancellationToken.None);

        var clock = new FakeTimeProvider(new DateTimeOffset(2026, 3, 15, 0, 0, 0, TimeSpan.Zero));
        var handler = new CreateMembershipHandler(db, clock);

        var result = await handler.Handle(member.Id, new CreateMembershipRequest(type.Id), CancellationToken.None);

        Assert.Equal(CreateMembershipStatus.Success, result.Status);
    }

    [Fact]
    public async Task ReturnsMemberNotFound_WhenMemberDoesNotExist()
    {
        await using var db = TestDbContextFactory.Create();
        var type = RollingType();
        db.MembershipTypes.Add(type);
        await db.SaveChangesAsync(CancellationToken.None);

        var clock = new FakeTimeProvider(DateTimeOffset.UtcNow);
        var handler = new CreateMembershipHandler(db, clock);

        var result = await handler.Handle(999, new CreateMembershipRequest(type.Id), CancellationToken.None);

        Assert.Equal(CreateMembershipStatus.MemberNotFound, result.Status);
    }
}
