namespace Membr.Module.Member.Application.Handlers.Contacts;

using Domain;
using Microsoft.EntityFrameworkCore;
using Persistence;

internal sealed class CreateMemberContactHandler(MembersDbContext db)
{
    public async Task<CreateMemberContactResult> Handle(int memberId, CreateMemberContactRequest request, CancellationToken ct)
    {
        var memberExists = await db.Members.AnyAsync(m => m.Id == memberId, ct);
        if (!memberExists)
            return CreateMemberContactResult.MemberNotFound();

        if (string.IsNullOrWhiteSpace(request.ContactDetail))
            return CreateMemberContactResult.Invalid("Contact detail is required.");

        if (request.IsPrimary)
            await ContactPrimaryHelper.ClearOtherPrimariesAsync(db, memberId, request.ContactType, null, ct);

        var contact = new ContactInformation
        {
            MemberId = memberId,
            ContactType = request.ContactType,
            ContactDetail = request.ContactDetail,
            IsPrimary = request.IsPrimary,
        };

        db.ContactInformation.Add(contact);
        await db.SaveChangesAsync(ct);

        return CreateMemberContactResult.Success(ContactDto.FromEntity(contact));
    }
}

internal enum CreateMemberContactStatus
{
    Success,
    MemberNotFound,
    Invalid,
}

internal sealed record CreateMemberContactResult(CreateMemberContactStatus Status, ContactDto? Contact, string? Error)
{
    public static CreateMemberContactResult Success(ContactDto dto) => new(CreateMemberContactStatus.Success, dto, null);
    public static CreateMemberContactResult MemberNotFound() => new(CreateMemberContactStatus.MemberNotFound, null, null);
    public static CreateMemberContactResult Invalid(string error) => new(CreateMemberContactStatus.Invalid, null, error);
}

internal sealed record CreateMemberContactRequest(ContactType ContactType, string ContactDetail, bool IsPrimary);
