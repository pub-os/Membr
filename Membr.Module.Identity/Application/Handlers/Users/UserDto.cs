using Membr.Module.Identity.Domain;

namespace Membr.Module.Identity.Application.Handlers.Users;

internal sealed record UserDto(string Id, string Email, string DisplayName, IReadOnlyList<string> Roles)
{
    public static UserDto FromEntity(ApplicationUser user, IList<string> roles) =>
        new(user.Id, user.Email ?? string.Empty, user.DisplayName, [.. roles]);
}
