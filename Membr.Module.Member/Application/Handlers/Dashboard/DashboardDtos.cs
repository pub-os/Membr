namespace Membr.Module.Member.Application.Handlers.Dashboard;

internal sealed record MembershipTypeBreakdownDto(int MembershipTypeId, string MembershipTypeName, int ActiveCount);

internal sealed record MonthlyActivityDto(int Year, int Month, int NewMemberships, int Renewals);

internal sealed record RecentlyJoinedMemberDto(
    int MemberId, string FirstName, string Surname, string MembershipTypeName, DateTime JoinedAt);

internal sealed record RecentRenewalDto(
    int MembershipId,
    int MemberId,
    string FirstName,
    string Surname,
    string MembershipTypeName,
    DateTime RenewedAt,
    DateTime NewEndDate);

internal sealed record DashboardStatsDto(
    int TotalMembers,
    int ActiveMembers,
    List<MembershipTypeBreakdownDto> MembershipTypeBreakdown,
    List<MonthlyActivityDto> MonthlyActivity,
    List<RecentlyJoinedMemberDto> RecentlyJoinedMembers,
    List<RecentRenewalDto> RecentRenewals);
