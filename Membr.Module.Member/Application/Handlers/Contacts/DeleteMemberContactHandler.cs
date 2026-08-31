namespace Membr.Module.Member.Application.Handlers.Contacts;

using Microsoft.EntityFrameworkCore;
using Persistence;

internal sealed class DeleteMemberContactHandler(MembersDbContext db)
{
    public async Task<bool> Handle(int memberId, int contactId, CancellationToken ct)
    {
        var contact = await db.ContactInformation.FirstOrDefaultAsync(c => c.Id == contactId && c.MemberId == memberId, ct);
        if (contact is null)
            return false;

        db.ContactInformation.Remove(contact);
        await db.SaveChangesAsync(ct);
        return true;
    }
}
