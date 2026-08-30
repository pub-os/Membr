using Membr.Module.Member.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Membr.Module.Member.Tests;

internal static class TestDbContextFactory
{
    public static MembersDbContext Create()
    {
        var options = new DbContextOptionsBuilder<MembersDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new MembersDbContext(options);
    }
}
