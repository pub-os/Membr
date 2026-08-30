namespace Membr.Module.Member.Persistence;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain;
internal sealed class MemberConfiguration : IEntityTypeConfiguration<Member>
{
    public void Configure(EntityTypeBuilder<Member> builder)
    {
        builder.ToTable("members");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(m => m.Surname).HasMaxLength(100).IsRequired();
        builder.Property(m => m.DateOfBirth);
    }
}
