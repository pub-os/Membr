namespace Membr.Module.Member.Application.Handlers.Contacts;

using Domain;
using Microsoft.EntityFrameworkCore;
using Persistence;

internal static class ContactPrimaryHelper
{
    public static async Task ClearOtherPrimariesAsync(
        MembersDbContext db, int memberId, ContactType contactType, int? excludeId, CancellationToken ct)
    {
        var others = await db.ContactInformation
            .Where(c => c.MemberId == memberId && c.ContactType == contactType && c.IsPrimary && c.Id != (excludeId ?? -1))
            .ToListAsync(ct);

        foreach (var other in others)
            other.IsPrimary = false;
    }
}
