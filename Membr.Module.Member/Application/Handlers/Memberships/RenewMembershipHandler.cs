namespace Membr.Module.Member.Application.Handlers.Memberships;

using Domain;
using Microsoft.EntityFrameworkCore;
using Persistence;

internal sealed class RenewMembershipHandler(MembersDbContext db, TimeProvider clock)
{
    public async Task<MembershipDto?> Handle(int memberId, int membershipId, CancellationToken ct)
    {
        var membership = await db.Memberships
            .Include(m => m.MembershipType)
            .FirstOrDefaultAsync(m => m.Id == membershipId && m.MemberId == memberId, ct);

        if (membership is null)
            return null;

        var now = clock.GetUtcNow().UtcDateTime;
        var previousEndDate = membership.EndDate;
        membership.EndDate = MembershipDateCalculator.CalculateNewEndDate(membership.MembershipType, now, membership.EndDate);

        db.MembershipRenewals.Add(new MembershipRenewal
        {
            MembershipId = membership.Id,
            RenewedAt = now,
            PreviousEndDate = previousEndDate,
            NewEndDate = membership.EndDate,
        });

        await db.SaveChangesAsync(ct);

        return MembershipDto.FromEntity(membership, now);
    }
}
