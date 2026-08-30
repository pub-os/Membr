namespace Membr.Module.Member.Application.Handlers.Dashboard;

using Microsoft.EntityFrameworkCore;
using Persistence;

internal sealed class GetDashboardStatsHandler(MembersDbContext db, TimeProvider clock)
{
    private const int MonthsOfHistory = 12;

    public async Task<DashboardStatsDto> Handle(CancellationToken ct)
    {
        var now = clock.GetUtcNow().UtcDateTime;
        var historyStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-(MonthsOfHistory - 1));

        var totalMembers = await db.Members.CountAsync(ct);

        var activeMembers = await db.Memberships
            .Where(m => m.EndDate >= now)
            .Select(m => m.MemberId)
            .Distinct()
            .CountAsync(ct);

        var breakdown = await db.Memberships
            .Where(m => m.EndDate >= now)
            .GroupBy(m => new { m.MembershipTypeId, m.MembershipType.Name })
            .Select(g => new { g.Key.MembershipTypeId, g.Key.Name, ActiveCount = g.Count() })
            .ToListAsync(ct);

        var membershipTypeBreakdown = breakdown
            .OrderByDescending(x => x.ActiveCount)
            .Select(x => new MembershipTypeBreakdownDto(x.MembershipTypeId, x.Name, x.ActiveCount))
            .ToList();

        var newMembershipsByMonth = await db.Memberships
            .Where(m => m.StartDate >= historyStart)
            .GroupBy(m => new { m.StartDate.Year, m.StartDate.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() })
            .ToListAsync(ct);

        var renewalsByMonth = await db.MembershipRenewals
            .Where(r => r.RenewedAt >= historyStart)
            .GroupBy(r => new { r.RenewedAt.Year, r.RenewedAt.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() })
            .ToListAsync(ct);

        var monthlyActivity = new List<MonthlyActivityDto>();
        for (var i = 0; i < MonthsOfHistory; i++)
        {
            var month = historyStart.AddMonths(i);
            var newCount = newMembershipsByMonth.FirstOrDefault(x => x.Year == month.Year && x.Month == month.Month)?.Count ?? 0;
            var renewalCount = renewalsByMonth.FirstOrDefault(x => x.Year == month.Year && x.Month == month.Month)?.Count ?? 0;
            monthlyActivity.Add(new MonthlyActivityDto(month.Year, month.Month, newCount, renewalCount));
        }

        return new DashboardStatsDto(totalMembers, activeMembers, membershipTypeBreakdown, monthlyActivity);
    }
}
