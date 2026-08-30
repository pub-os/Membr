using Membr.Module.Identity.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Membr.Module.Identity.Application.Handlers.Users;

internal sealed class ListUsersHandler(UserManager<ApplicationUser> userManager)
{
    public async Task<List<UserDto>> Handle(CancellationToken ct)
    {
        var users = await userManager.Users.OrderBy(u => u.DisplayName).ToListAsync(ct);

        var result = new List<UserDto>(users.Count);
        foreach (var user in users)
        {
            var roles = await userManager.GetRolesAsync(user);
            result.Add(UserDto.FromEntity(user, roles));
        }

        return result;
    }
}
