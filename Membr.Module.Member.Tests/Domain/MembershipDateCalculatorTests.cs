using Membr.Module.Member.Domain;

namespace Membr.Module.Member.Tests.Domain;

public class MembershipDateCalculatorTests
{
    private static MembershipType RollingType(int durationMonths) => new()
    {
        Id = 1,
        Name = "Rolling",
        IsActive = true,
        RenewalMode = MembershipRenewalMode.Rolling,
        DurationMonths = durationMonths,
    };

    private static MembershipType FixedTermType(int anchorMonth, int anchorDay) => new()
    {
        Id = 2,
        Name = "Fixed",
        IsActive = true,
        RenewalMode = MembershipRenewalMode.FixedTerm,
        FixedTermAnchorMonth = anchorMonth,
        FixedTermAnchorDay = anchorDay,
    };

    private static DateTime Utc(int year, int month, int day) => new(year, month, day, 0, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Rolling_NewMembership_StartsFromNow()
    {
        var type = RollingType(12);
        var now = Utc(2026, 3, 15);

        var end = MembershipDateCalculator.CalculateNewEndDate(type, now, currentEndDate: null);

        Assert.Equal(Utc(2027, 3, 15), end);
    }

    [Fact]
    public void Rolling_RenewWhileActive_ExtendsFromCurrentEndDate()
    {
        var type = RollingType(1);
        var now = Utc(2026, 3, 15);
        var currentEnd = Utc(2026, 4, 1);

        var end = MembershipDateCalculator.CalculateNewEndDate(type, now, currentEnd);

        Assert.Equal(Utc(2026, 5, 1), end);
    }

    [Fact]
    public void Rolling_RenewAfterExpiry_ExtendsFromNow()
    {
        var type = RollingType(1);
        var now = Utc(2026, 5, 1);
        var currentEnd = Utc(2026, 4, 1);

        var end = MembershipDateCalculator.CalculateNewEndDate(type, now, currentEnd);

        Assert.Equal(Utc(2026, 6, 1), end);
    }

    [Fact]
    public void FixedTerm_RenewWhileActive_AddsOneYearKeepingAnchor()
    {
        // Member has an active membership ending 1 Jan next year and renews in July.
        var type = FixedTermType(1, 1);
        var now = Utc(2026, 7, 15);
        var currentEnd = Utc(2027, 1, 1);

        var end = MembershipDateCalculator.CalculateNewEndDate(type, now, currentEnd);

        Assert.Equal(Utc(2028, 1, 1), end);
    }

    [Fact]
    public void FixedTerm_RenewAfterExpiry_JumpsToNextAnchor()
    {
        // Membership expired 1 Jan this year, member renews in May -> valid until 1 Jan next year.
        var type = FixedTermType(1, 1);
        var now = Utc(2026, 5, 1);
        var currentEnd = Utc(2026, 1, 1);

        var end = MembershipDateCalculator.CalculateNewEndDate(type, now, currentEnd);

        Assert.Equal(Utc(2027, 1, 1), end);
    }

    [Fact]
    public void FixedTerm_NewMembership_BeforeAnchorThisYear_UsesThisYearsAnchor()
    {
        var type = FixedTermType(1, 31);
        var now = Utc(2026, 1, 10);

        var end = MembershipDateCalculator.CalculateNewEndDate(type, now, currentEndDate: null);

        Assert.Equal(Utc(2026, 1, 31), end);
    }

    [Fact]
    public void FixedTerm_NewMembership_AfterAnchorThisYear_UsesNextYearsAnchor()
    {
        var type = FixedTermType(1, 31);
        var now = Utc(2026, 6, 1);

        var end = MembershipDateCalculator.CalculateNewEndDate(type, now, currentEndDate: null);

        Assert.Equal(Utc(2027, 1, 31), end);
    }

    [Fact]
    public void FixedTerm_AnchorOnLeapDay_ClampsToLastDayOfFebruaryInNonLeapYear()
    {
        _ = FixedTermType(2, 29);
        var now = Utc(2027, 1, 1); // 2027 is not a leap year

        var end = MembershipDateCalculator.NextAnchorOnOrAfter(now, 2, 29);

        Assert.Equal(Utc(2027, 2, 28), end);
    }

    [Fact]
    public void FixedTerm_AnchorOnLeapDay_UsesFeb29InLeapYear()
    {
        var now = Utc(2028, 1, 1); // 2028 is a leap year

        var end = MembershipDateCalculator.NextAnchorOnOrAfter(now, 2, 29);

        Assert.Equal(Utc(2028, 2, 29), end);
    }
}
