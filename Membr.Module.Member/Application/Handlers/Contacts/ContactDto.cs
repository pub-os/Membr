namespace Membr.Module.Member.Application.Handlers.Contacts;

using Domain;

internal sealed record ContactDto(int Id, ContactType ContactType, string ContactDetail, bool IsPrimary)
{
    public static ContactDto FromEntity(ContactInformation c) => new(c.Id, c.ContactType, c.ContactDetail, c.IsPrimary);
}
