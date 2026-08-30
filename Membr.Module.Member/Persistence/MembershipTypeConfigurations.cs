using Membr.Module.Member.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Membr.Module.Member.Persistence;

public class MembershipTypeConfigurations : IEntityTypeConfiguration<MembershipType>
{
    public void Configure(EntityTypeBuilder<MembershipType> builder)
    {
        builder.ToTable("membership_types");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Name).HasMaxLength(100).IsRequired();
        builder.Property(m => m.Description).HasMaxLength(500);
        builder.Property(m => m.IsActive).HasDefaultValue(true);

        builder.Property(m => m.RenewalMode).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(m => m.DurationMonths);
        builder.Property(m => m.FixedTermAnchorMonth);
        builder.Property(m => m.FixedTermAnchorDay);
    }
}
