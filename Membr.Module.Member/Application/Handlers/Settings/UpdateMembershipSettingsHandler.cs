namespace Membr.Module.Member.Application.Handlers.Settings;

using Domain;
using Persistence;

internal sealed class UpdateMembershipSettingsHandler(MembersDbContext db)
{
    public async Task<MembershipSettingsDto> Handle(UpdateMembershipSettingsRequest request, CancellationToken ct)
    {
        var settings = await db.MembershipSettings.FindAsync([MembershipSettings.SingletonId], ct);
        if (settings is null)
        {
            settings = new MembershipSettings();
            db.MembershipSettings.Add(settings);
        }

        settings.AllowMultipleMemberships = request.AllowMultipleMemberships;
        await db.SaveChangesAsync(ct);

        return MembershipSettingsDto.FromEntity(settings);
    }
}

internal sealed record UpdateMembershipSettingsRequest(bool AllowMultipleMemberships);
