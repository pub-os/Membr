namespace Membr.Module.Member.Persistence;

using Bogus;
using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

internal static class MemberSeeder
{
    public static async Task SeedAsync(IServiceProvider services, int count, CancellationToken ct = default)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MembersDbContext>();

        var membershipTypes = await SeedMembershipTypesAsync(db, ct);
        var udfDefinitions = await SeedUdfDefinitionsAsync(db, ct);

        var memberFaker = new Faker<Member>()
            .RuleFor(m => m.FirstName, f => f.Name.FirstName())
            .RuleFor(m => m.Surname, f => f.Name.LastName())
            .RuleFor(m => m.DateOfBirth, f => DateOnly.FromDateTime(f.Date.Between(
                DateTime.SpecifyKind(new DateTime(1940, 1, 1), DateTimeKind.Utc),
                DateTime.SpecifyKind(new DateTime(2008, 12, 31), DateTimeKind.Utc))));

        var faker = new Faker();
        var now = DateTime.UtcNow;

        var marketingOptOut = udfDefinitions["Marketing - Opt out"];
        var rfidCollected = udfDefinitions["RFID Card Collected"];
        var deluxeLastPerk = udfDefinitions["Deluxe - Last Perk Claimed"];
        var ultraLastPerk = udfDefinitions["Ultra - Last Perk Claimed"];

        const int batchSize = 500;
        for (var batchStart = 0; batchStart < count; batchStart += batchSize)
        {
            var batchCount = Math.Min(batchSize, count - batchStart);
            var members = memberFaker.Generate(batchCount);
            db.Members.AddRange(members);
            await db.SaveChangesAsync(ct);

            foreach (var member in members)
            {
                var membershipType = faker.PickRandom(membershipTypes);
                var startDate = AsUtc(faker.Date.Past(2, now));
                var endDate = startDate.AddMonths(membershipType.DurationMonths!.Value);
                db.Memberships.Add(new Membership
                {
                    MemberId = member.Id,
                    MembershipTypeId = membershipType.Id,
                    StartDate = startDate,
                    EndDate = endDate,
                });

                var email = faker.Internet.Email(member.FirstName, member.Surname);
                db.ContactInformation.AddRange(
                    new ContactInformation
                    {
                        MemberId = member.Id,
                        ContactType = ContactType.Email,
                        ContactDetail = email,
                        IsPrimary = true,
                    },
                    new ContactInformation
                    {
                        MemberId = member.Id,
                        ContactType = ContactType.Phone,
                        ContactDetail = faker.Phone.PhoneNumber("07#########"),
                        IsPrimary = false,
                    });

                var optedOut = faker.Random.Bool(0.3f);
                db.MemberUdfValues.Add(new MemberUdfValue
                {
                    MemberId = member.Id,
                    UdfDefinitionId = marketingOptOut.Id,
                    Value = optedOut ? "true" : "false",
                });

                var hasRfidCard = faker.Random.Bool(0.8f);
                if (hasRfidCard)
                {
                    var collectedAt = AsUtc(faker.Date.Between(startDate, now));
                    db.Tokens.Add(new Token
                    {
                        MemberId = member.Id,
                        TokenType = TokenType.Rfid,
                        Value = $"RFID-{faker.Random.Hexadecimal(12, "")}".ToUpperInvariant(),
                        IsRevoked = false,
                        CreatedAt = collectedAt,
                    });

                    db.MemberUdfValues.Add(new MemberUdfValue
                    {
                        MemberId = member.Id,
                        UdfDefinitionId = rfidCollected.Id,
                        Value = collectedAt.ToString("yyyy-MM-dd"),
                    });
                }

                if (membershipType.Name == "Deluxe")
                {
                    db.MemberUdfValues.Add(new MemberUdfValue
                    {
                        MemberId = member.Id,
                        UdfDefinitionId = deluxeLastPerk.Id,
                        Value = faker.Date.Between(startDate, now).ToString("yyyy-MM-dd"),
                    });
                }
                else if (membershipType.Name == "Ultra")
                {
                    db.MemberUdfValues.Add(new MemberUdfValue
                    {
                        MemberId = member.Id,
                        UdfDefinitionId = ultraLastPerk.Id,
                        Value = faker.Date.Between(startDate, now).ToString("yyyy-MM-dd"),
                    });
                }
            }

            await db.SaveChangesAsync(ct);
        }
    }

    private static DateTime AsUtc(DateTime value) => DateTime.SpecifyKind(value, DateTimeKind.Utc);

    private static async Task<List<MembershipType>> SeedMembershipTypesAsync(MembersDbContext db, CancellationToken ct)
    {
        var existing = await db.MembershipTypes
            .Where(t => t.Name == "Standard" || t.Name == "Ultra" || t.Name == "Deluxe")
            .ToListAsync(ct);

        var toCreate = new[]
            {
                new MembershipType
                {
                    Name = "Standard",
                    Description = "Standard membership with core benefits.",
                    IsActive = true,
                    RenewalMode = MembershipRenewalMode.Rolling,
                    DurationMonths = 1,
                },
                new MembershipType
                {
                    Name = "Ultra",
                    Description = "Ultra membership with extended perks.",
                    IsActive = true,
                    RenewalMode = MembershipRenewalMode.Rolling,
                    DurationMonths = 12,
                },
                new MembershipType
                {
                    Name = "Deluxe",
                    Description = "Deluxe membership with premium perks.",
                    IsActive = true,
                    RenewalMode = MembershipRenewalMode.Rolling,
                    DurationMonths = 12,
                },
            }
            .Where(t => existing.All(e => e.Name != t.Name))
            .ToList();

        if (toCreate.Count > 0)
        {
            db.MembershipTypes.AddRange(toCreate);
            await db.SaveChangesAsync(ct);
        }

        return [.. existing, .. toCreate];
    }

    private static async Task<Dictionary<string, UdfDefinition>> SeedUdfDefinitionsAsync(MembersDbContext db, CancellationToken ct)
    {
        var names = new[]
        {
            "Marketing - Opt out",
            "RFID Card Collected",
            "Deluxe - Last Perk Claimed",
            "Ultra - Last Perk Claimed",
        };

        var existing = await db.UdfDefinitions.Where(d => names.Contains(d.Name)).ToListAsync(ct);

        var toCreate = new[]
            {
                new UdfDefinition { Name = "Marketing - Opt out", Type = UdfFieldType.Bool, IsActive = true, DefaultValue = "false" },
                new UdfDefinition { Name = "RFID Card Collected", Type = UdfFieldType.Date, IsActive = true },
                new UdfDefinition { Name = "Deluxe - Last Perk Claimed", Type = UdfFieldType.Date, IsActive = true },
                new UdfDefinition { Name = "Ultra - Last Perk Claimed", Type = UdfFieldType.Date, IsActive = true },
            }
            .Where(d => existing.All(e => e.Name != d.Name))
            .ToList();

        if (toCreate.Count > 0)
        {
            db.UdfDefinitions.AddRange(toCreate);
            await db.SaveChangesAsync(ct);
        }

        return existing.Concat(toCreate).ToDictionary(d => d.Name);
    }
}
