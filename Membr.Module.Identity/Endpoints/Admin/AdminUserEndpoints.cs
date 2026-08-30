namespace Membr.Module.Identity.Endpoints.Admin;

using Application.Handlers.Users;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

internal static class AdminUserEndpoints
{
    public static void MapAdminUserRoutes(this IEndpointRouteBuilder app)
    {
        var users = app.MapGroup("/admin/users")
            .WithTags("Admin: Users")
            .RequireAuthorization("AdminOnly");

        users.MapGet("/", ListUsers)
            .WithName("ListUsers")
            .WithSummary("List admin users");

        users.MapPost("/", CreateUser)
            .WithName("CreateUser")
            .WithSummary("Create a new admin user");

        users.MapGet("/roles", ListRoles)
            .WithName("ListRoles")
            .WithSummary("List available roles");
    }

    private static async Task<Ok<List<UserDto>>> ListUsers(ListUsersHandler handler, CancellationToken ct)
    {
        var users = await handler.Handle(ct);
        return TypedResults.Ok(users);
    }

    private static async Task<Results<Created<UserDto>, ValidationProblem>> CreateUser(
        CreateUserRequest request, CreateUserHandler handler, CancellationToken ct)
    {
        var result = await handler.Handle(request, ct);
        if (result.User is null)
            return TypedResults.ValidationProblem(new Dictionary<string, string[]> { ["email"] = [result.Error!] });

        return TypedResults.Created($"/admin/users/{result.User.Id}", result.User);
    }

    private static async Task<Ok<List<string>>> ListRoles(ListRolesHandler handler, CancellationToken ct)
    {
        var roles = await handler.Handle(ct);
        return TypedResults.Ok(roles);
    }
}
