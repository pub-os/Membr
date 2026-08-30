namespace Membr.Module.Member.Application.Handlers.Settings;

using Domain;
using Persistence;

internal sealed class GetMembershipSettingsHandler(MembersDbContext db)
{
    public async Task<MembershipSettingsDto> Handle(CancellationToken ct)
    {
        var settings = await db.MembershipSettings.FindAsync([MembershipSettings.SingletonId], ct)
            ?? new MembershipSettings();

        return MembershipSettingsDto.FromEntity(settings);
    }
}
