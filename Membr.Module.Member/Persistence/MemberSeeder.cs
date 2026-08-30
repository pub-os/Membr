namespace Membr.Module.Member.Persistence;

using Bogus;
using Domain;
using Microsoft.Extensions.DependencyInjection;

internal static class MemberSeeder
{
    public static async Task SeedAsync(IServiceProvider services, int count, CancellationToken ct = default)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MembersDbContext>();

        var faker = new Faker<Member>()
            .RuleFor(m => m.FirstName, f => f.Name.FirstName())
            .RuleFor(m => m.Surname, f => f.Name.LastName())
            .RuleFor(m => m.DateOfBirth, f => DateOnly.FromDateTime(f.Date.Between(
                DateTime.SpecifyKind(new DateTime(1940, 1, 1), DateTimeKind.Utc),
                DateTime.SpecifyKind(new DateTime(2008, 12, 31), DateTimeKind.Utc))));

        const int batchSize = 500;
        for (var batchStart = 0; batchStart < count; batchStart += batchSize)
        {
            var batchCount = Math.Min(batchSize, count - batchStart);
            db.Members.AddRange(faker.Generate(batchCount));
            await db.SaveChangesAsync(ct);
        }
    }
}
