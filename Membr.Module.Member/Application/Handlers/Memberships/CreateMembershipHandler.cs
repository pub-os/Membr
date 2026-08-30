namespace Membr.Module.Member.Application.Handlers.Memberships;

using Domain;
using Microsoft.EntityFrameworkCore;
using Persistence;

internal enum CreateMembershipStatus
{
    Success,
    MemberNotFound,
    MembershipTypeNotFound,
    MultipleMembershipsNotAllowed,
}

internal sealed record CreateMembershipResult(CreateMembershipStatus Status, MembershipDto? Membership = null, string? Error = null)
{
    public static CreateMembershipResult Success(MembershipDto dto) => new(CreateMembershipStatus.Success, dto);
    public static CreateMembershipResult Fail(CreateMembershipStatus status, string error) => new(status, Error: error);
}

internal sealed class CreateMembershipHandler(MembersDbContext db, TimeProvider clock)
{
    public async Task<CreateMembershipResult> Handle(int memberId, CreateMembershipRequest request, CancellationToken ct)
    {
        var member = await db.Members.FindAsync([memberId], ct);
        if (member is null)
            return CreateMembershipResult.Fail(CreateMembershipStatus.MemberNotFound, "Member not found.");

        var type = await db.MembershipTypes.FindAsync([request.MembershipTypeId], ct);
        if (type is null)
            return CreateMembershipResult.Fail(CreateMembershipStatus.MembershipTypeNotFound, "Membership type not found.");

        var now = clock.GetUtcNow().UtcDateTime;

        var settings = await db.MembershipSettings.FindAsync([MembershipSettings.SingletonId], ct)
            ?? new MembershipSettings();

        if (!settings.AllowMultipleMemberships)
        {
            var hasActiveMembership = await db.Memberships
                .Where(m => m.MemberId == memberId && m.EndDate >= now)
                .AnyAsync(ct);

            if (hasActiveMembership)
            {
                return CreateMembershipResult.Fail(
                    CreateMembershipStatus.MultipleMembershipsNotAllowed,
                    "This member already has an active membership and multiple memberships are not allowed.");
            }
        }

        var membership = new Membership
        {
            MemberId = memberId,
            MembershipTypeId = type.Id,
            MembershipType = type,
            StartDate = now,
            EndDate = MembershipDateCalculator.CalculateNewEndDate(type, now, currentEndDate: null),
        };

        db.Memberships.Add(membership);
        await db.SaveChangesAsync(ct);

        return CreateMembershipResult.Success(MembershipDto.FromEntity(membership, now));
    }
}

internal sealed record CreateMembershipRequest(int MembershipTypeId);
