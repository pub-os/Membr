using Membr.Module.Member.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Membr.Module.Member.Persistence;

public class MembershipRenewalConfiguration : IEntityTypeConfiguration<MembershipRenewal>
{
    public void Configure(EntityTypeBuilder<MembershipRenewal> builder)
    {
        builder.ToTable("membership_renewals");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.RenewedAt);
        builder.Property(x => x.PreviousEndDate);
        builder.Property(x => x.NewEndDate);

        builder.HasOne(x => x.Membership)
            .WithMany()
            .HasForeignKey(x => x.MembershipId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
