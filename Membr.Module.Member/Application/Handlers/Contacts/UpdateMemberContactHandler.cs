namespace Membr.Module.Member.Application.Handlers.Contacts;

using Domain;
using Microsoft.EntityFrameworkCore;
using Persistence;

internal sealed class UpdateMemberContactHandler(MembersDbContext db)
{
    public async Task<UpdateMemberContactResult> Handle(int memberId, int contactId, UpdateMemberContactRequest request, CancellationToken ct)
    {
        var contact = await db.ContactInformation.FirstOrDefaultAsync(c => c.Id == contactId && c.MemberId == memberId, ct);
        if (contact is null)
            return UpdateMemberContactResult.NotFound();

        if (string.IsNullOrWhiteSpace(request.ContactDetail))
            return UpdateMemberContactResult.Invalid("Contact detail is required.");

        if (request.IsPrimary)
            await ContactPrimaryHelper.ClearOtherPrimariesAsync(db, memberId, request.ContactType, contact.Id, ct);

        contact.ContactType = request.ContactType;
        contact.ContactDetail = request.ContactDetail;
        contact.IsPrimary = request.IsPrimary;

        await db.SaveChangesAsync(ct);

        return UpdateMemberContactResult.Success(ContactDto.FromEntity(contact));
    }
}

internal enum UpdateMemberContactStatus
{
    Success,
    NotFound,
    Invalid,
}

internal sealed record UpdateMemberContactResult(UpdateMemberContactStatus Status, ContactDto? Contact, string? Error)
{
    public static UpdateMemberContactResult Success(ContactDto dto) => new(UpdateMemberContactStatus.Success, dto, null);
    public static UpdateMemberContactResult NotFound() => new(UpdateMemberContactStatus.NotFound, null, null);
    public static UpdateMemberContactResult Invalid(string error) => new(UpdateMemberContactStatus.Invalid, null, error);
}

internal sealed record UpdateMemberContactRequest(ContactType ContactType, string ContactDetail, bool IsPrimary);
