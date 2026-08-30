namespace Membr.Module.Member.Persistence;

using Microsoft.EntityFrameworkCore;
using Domain;

internal sealed class MembersDbContext(DbContextOptions<MembersDbContext> options) : DbContext(options)
{
    public const string Schema = "members";
    private const string VersionSequence = "members.entity_version_seq";
    private const string MemberNumberSequence = "members.member_number_seq";

    public DbSet<Member> Members => Set<Member>();
    public DbSet<MembershipType> MembershipTypes => Set<MembershipType>();
    public DbSet<Membership> Memberships => Set<Membership>();
    public DbSet<MembershipRenewal> MembershipRenewals => Set<MembershipRenewal>();
    public DbSet<MembershipSettings> MembershipSettings => Set<MembershipSettings>();
    public DbSet<UdfDefinition> UdfDefinitions => Set<UdfDefinition>();
    public DbSet<MemberUdfValue> MemberUdfValues => Set<MemberUdfValue>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema(Schema);
        builder.ApplyConfigurationsFromAssembly(typeof(MembersDbContext).Assembly);
    }
}
