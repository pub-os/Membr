namespace Membr.Module.Member.Application.Handlers.Dashboard;

internal sealed record MembershipTypeBreakdownDto(int MembershipTypeId, string MembershipTypeName, int ActiveCount);

internal sealed record MonthlyActivityDto(int Year, int Month, int NewMemberships, int Renewals);

internal sealed record DashboardStatsDto(
    int TotalMembers,
    int ActiveMembers,
    List<MembershipTypeBreakdownDto> MembershipTypeBreakdown,
    List<MonthlyActivityDto> MonthlyActivity);
