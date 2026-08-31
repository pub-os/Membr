using Membr.Module.Member.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Membr.Module.Member.Persistence;

internal sealed class TokenConfigurations : IEntityTypeConfiguration<Token>
{
    public void Configure(EntityTypeBuilder<Token> builder)
    {
        builder.ToTable("tokens");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.TokenType).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(t => t.Value).HasMaxLength(100).IsRequired();
        builder.Property(t => t.IsRevoked).HasDefaultValue(false);

        builder.HasOne(t => t.Member)
            .WithMany()
            .HasForeignKey(t => t.MemberId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(t => t.Value).IsUnique();
        builder.HasIndex(t => t.MemberId);
    }
}
