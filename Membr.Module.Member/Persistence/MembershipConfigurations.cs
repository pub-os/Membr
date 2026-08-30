using Membr.Module.Member.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Membr.Module.Member.Persistence;

public class MembershipConfigurations : IEntityTypeConfiguration<Membership>
{
    public void Configure(EntityTypeBuilder<Membership> builder)
    {
        builder.ToTable("memberships");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.StartDate);
        builder.Property(x => x.EndDate);

        builder.HasOne(x => x.Member)
            .WithMany()
            .HasForeignKey(x => x.MemberId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.MembershipType)
            .WithMany()
            .HasForeignKey(x => x.MembershipTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
