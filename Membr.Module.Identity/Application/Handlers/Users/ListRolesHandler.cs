using Membr.Module.Identity.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Membr.Module.Identity.Application.Handlers.Users;

internal sealed class ListRolesHandler(RoleManager<ApplicationRole> roleManager)
{
    public Task<List<string>> Handle(CancellationToken ct) =>
        roleManager.Roles
            .Where(r => r.Enabled)
            .OrderBy(r => r.Name)
            .Select(r => r.Name!)
            .ToListAsync(ct);
}
