using Membr.Module.Member.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Membr.Module.Member.Persistence;

public class MembershipSettingsConfigurations : IEntityTypeConfiguration<MembershipSettings>
{
    public void Configure(EntityTypeBuilder<MembershipSettings> builder)
    {
        builder.ToTable("membership_settings");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.AllowMultipleMemberships).HasDefaultValue(false);

        builder.HasData(new MembershipSettings
        {
            Id = MembershipSettings.SingletonId,
            AllowMultipleMemberships = false,
        });
    }
}
