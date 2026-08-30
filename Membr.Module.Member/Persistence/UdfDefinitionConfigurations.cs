using System.Text.Json;
using Membr.Module.Member.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Membr.Module.Member.Persistence;

public class UdfDefinitionConfigurations : IEntityTypeConfiguration<UdfDefinition>
{
    public void Configure(EntityTypeBuilder<UdfDefinition> builder)
    {
        builder.ToTable("udf_definitions");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Name).HasMaxLength(100).IsRequired();
        builder.Property(d => d.Type).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(d => d.IsActive).HasDefaultValue(true);
        builder.Property(d => d.DefaultValue).HasMaxLength(2000);

        builder.Property(d => d.Options)
            .HasConversion(
                options => JsonSerializer.Serialize(options, (JsonSerializerOptions?)null),
                json => string.IsNullOrEmpty(json)
                    ? new List<string>()
                    : JsonSerializer.Deserialize<List<string>>(json, (JsonSerializerOptions?)null)!)
            .Metadata.SetValueComparer(new ValueComparer<List<string>>(
                (a, b) => (a ?? new()).SequenceEqual(b ?? new()),
                v => v.Aggregate(0, (hash, s) => HashCode.Combine(hash, s.GetHashCode())),
                v => v.ToList()));

        builder.HasIndex(d => d.Name).IsUnique();
    }
}
