using Membr.Module.Member.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Membr.Module.Member.Persistence;

public class MemberUdfValueConfigurations : IEntityTypeConfiguration<MemberUdfValue>
{
    public void Configure(EntityTypeBuilder<MemberUdfValue> builder)
    {
        builder.ToTable("member_udf_values");
        builder.HasKey(v => v.Id);
        builder.Property(v => v.Value).HasMaxLength(2000);

        builder.HasOne(v => v.Member)
            .WithMany()
            .HasForeignKey(v => v.MemberId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(v => v.UdfDefinition)
            .WithMany()
            .HasForeignKey(v => v.UdfDefinitionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(v => new { v.MemberId, v.UdfDefinitionId }).IsUnique();
    }
}
