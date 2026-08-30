using Membr.Module.Identity.Domain;
using Microsoft.AspNetCore.Identity;

namespace Membr.Module.Identity.Application.Handlers.Users;

internal sealed class CreateUserHandler(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
{
    public async Task<CreateUserResult> Handle(CreateUserRequest request, CancellationToken ct)
    {
        if (!await roleManager.RoleExistsAsync(request.Role))
            return CreateUserResult.Invalid($"Role '{request.Role}' does not exist.");

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            DisplayName = request.DisplayName,
            EmailConfirmed = true
        };

        var createResult = await userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
            return CreateUserResult.Invalid(string.Join(" ", createResult.Errors.Select(e => e.Description)));

        var roleResult = await userManager.AddToRoleAsync(user, request.Role);
        if (!roleResult.Succeeded)
        {
            await userManager.DeleteAsync(user);
            return CreateUserResult.Invalid(string.Join(" ", roleResult.Errors.Select(e => e.Description)));
        }

        return CreateUserResult.Success(UserDto.FromEntity(user, [request.Role]));
    }
}

internal sealed record CreateUserRequest(string Email, string DisplayName, string Password, string Role);

internal sealed record CreateUserResult(UserDto? User, string? Error)
{
    public static CreateUserResult Success(UserDto dto) => new(dto, null);
    public static CreateUserResult Invalid(string error) => new(null, error);
}
