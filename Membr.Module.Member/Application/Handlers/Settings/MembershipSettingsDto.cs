namespace Membr.Module.Member.Application.Handlers.Settings;

using Domain;

internal sealed record MembershipSettingsDto(bool AllowMultipleMemberships)
{
    public static MembershipSettingsDto FromEntity(MembershipSettings s) => new(s.AllowMultipleMemberships);
}
