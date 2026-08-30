namespace Membr.Module.Member.Domain;

/// <summary>
/// Pure calculation of a membership's new end date. Kept separate from the DbContext/handlers
/// so it's trivially unit-testable, and so future eligibility rules (age, tenure, etc.) have a
/// single place to plug into without touching persistence or endpoint code.
/// </summary>
public static class MembershipDateCalculator
{
    /// <summary>
    /// The next occurrence of the given calendar day (month/day) on or after <paramref name="utcToday"/>.
    /// A day that doesn't exist in a given month/year (e.g. Feb 31) is clamped to that month's last day.
    /// </summary>
    public static DateTime NextAnchorOnOrAfter(DateTime utcToday, int month, int day)
    {
        var today = utcToday.Date;
        var thisYearAnchor = AnchorDate(today.Year, month, day);
        return thisYearAnchor >= today ? thisYearAnchor : AnchorDate(today.Year + 1, month, day);
    }

    /// <summary>
    /// Computes the new end date for a membership of <paramref name="type"/>.
    /// <paramref name="currentEndDate"/> is null for a brand-new membership.
    /// </summary>
    public static DateTime CalculateNewEndDate(MembershipType type, DateTime utcNow, DateTime? currentEndDate)
    {
        return type.RenewalMode switch
        {
            MembershipRenewalMode.Rolling => CalculateRollingEndDate(type, utcNow, currentEndDate),
            MembershipRenewalMode.FixedTerm => CalculateFixedTermEndDate(type, utcNow, currentEndDate),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type.RenewalMode, "Unknown renewal mode"),
        };
    }

    private static DateTime CalculateRollingEndDate(MembershipType type, DateTime utcNow, DateTime? currentEndDate)
    {
        if (type.DurationMonths is not { } months)
            throw new InvalidOperationException("Rolling membership types must have a DurationMonths.");

        var baseline = currentEndDate is { } end && end >= utcNow ? end : utcNow.Date;
        return baseline.AddMonths(months);
    }

    private static DateTime CalculateFixedTermEndDate(MembershipType type, DateTime utcNow, DateTime? currentEndDate)
    {
        if (type.FixedTermAnchorMonth is not { } month || type.FixedTermAnchorDay is not { } day)
            throw new InvalidOperationException("Fixed-term membership types must have an anchor month and day.");

        if (currentEndDate is { } end && end >= utcNow)
            return end.AddYears(1);

        return NextAnchorOnOrAfter(utcNow, month, day);
    }

    private static DateTime AnchorDate(int year, int month, int day)
    {
        var clampedDay = Math.Min(day, DateTime.DaysInMonth(year, month));
        return new DateTime(year, month, clampedDay, 0, 0, 0, DateTimeKind.Utc);
    }
}
