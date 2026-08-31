namespace Membr.Module.Member.Application.Handlers.Contacts;

using Microsoft.EntityFrameworkCore;
using Persistence;

internal sealed class ListMemberContactsHandler(MembersDbContext db)
{
    public async Task<List<ContactDto>> Handle(int memberId, CancellationToken ct)
    {
        var contacts = await db.ContactInformation
            .Where(c => c.MemberId == memberId)
            .OrderByDescending(c => c.IsPrimary)
            .ThenBy(c => c.ContactType)
            .ToListAsync(ct);

        return [.. contacts.Select(ContactDto.FromEntity)];
    }
}
