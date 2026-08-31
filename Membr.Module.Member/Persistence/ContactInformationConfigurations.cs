using Membr.Module.Member.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Membr.Module.Member.Persistence;

internal sealed class ContactInformationConfigurations : IEntityTypeConfiguration<ContactInformation>
{
    public void Configure(EntityTypeBuilder<ContactInformation> builder)
    {
        builder.ToTable("contact_information");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.ContactType).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(c => c.ContactDetail).HasMaxLength(320).IsRequired();
        builder.Property(c => c.IsPrimary).HasDefaultValue(false);

        builder.HasOne(c => c.Member)
            .WithMany()
            .HasForeignKey(c => c.MemberId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(c => new { c.MemberId, c.ContactType });
    }
}
