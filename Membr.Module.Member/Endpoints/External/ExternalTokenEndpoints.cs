namespace Membr.Module.Member.Endpoints.External;

using Application.Handlers.Tokens;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

internal static class ExternalTokenEndpoints
{
    public static void MapMemberExternalRoutes(this IEndpointRouteBuilder app)
    {
        var members = app.MapGroup("/integrations/members")
            .WithTags("Integrations: Members")
            .RequireAuthorization("DeviceAccess");

        members.MapGet("/by-token/{value}", LookupMemberByToken)
            .WithName("LookupMemberByToken")
            .WithSummary("Resolve a scanned token to its member and membership information");
    }

    private static async Task<Results<Ok<TokenLookupDto>, NotFound>> LookupMemberByToken(
        [FromRoute] string value, LookupTokenHandler handler, CancellationToken ct)
    {
        var result = await handler.Handle(value, ct);
        if (result is null) return TypedResults.NotFound();
        return TypedResults.Ok(result);
    }
}
