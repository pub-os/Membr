// Endpoints/Admin/AdminEndpoints.cs
namespace Membr.Module.Member.Endpoints.Admin;

using Application.Handlers.Contacts;
using Application.Handlers.Dashboard;
using Application.Handlers.Members;
using Application.Handlers.Memberships;
using Application.Handlers.MembershipTypes;
using Application.Handlers.Settings;
using Application.Handlers.Tokens;
using Application.Handlers.Udf;
using Membr.Shared;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;


internal static class AdminEndpoints
{
    public static void MapMemberAdminRoutes(this IEndpointRouteBuilder app)
    {
        var members = app.MapGroup("/admin/members")
            .WithTags("Admin: Members")
            .RequireAuthorization("AdminOnly");

        members.MapPost("/", CreateMember)
            .WithName("CreateMember")
            .WithSummary("Create a new member");

        members.MapGet("/search", SearchMembers)
            .WithName("SearchMembers")
            .WithSummary("Search members");
        members.MapGet("/{id}", GetMember)
            .WithName("GetMember")
            .WithSummary("Retrieve member");
        members.MapGet("/", ListMembers)
            .WithName("ListMembers")
            .WithSummary("List members");

        var memberships = app.MapGroup("/admin/members/{memberId}/memberships")
            .WithTags("Admin: Memberships")
            .RequireAuthorization("AdminOnly");

        memberships.MapPost("/", CreateMembership)
            .WithName("CreateMembership")
            .WithSummary("Grant a member a new membership");
        memberships.MapGet("/", ListMemberMemberships)
            .WithName("ListMemberMemberships")
            .WithSummary("List a member's memberships");
        memberships.MapPost("/{membershipId}/renew", RenewMembership)
            .WithName("RenewMembership")
            .WithSummary("Renew an existing membership");

        var membershipTypes = app.MapGroup("/admin/membershiptypes")
            .WithTags("Admin: Membership Types")
            .RequireAuthorization("AdminOnly");
        membershipTypes.MapPost("/", CreateMembershipType)
            .WithName("CreateMembershipType")
            .WithSummary("Create a new membership type");
        membershipTypes.MapGet("/", ListMembershipTypes)
            .WithName("ListMembershipTypes")
            .WithSummary("List membership types");
        membershipTypes.MapGet("/{Id}", GetMembershipType)
            .WithName("GetMembershipType")
            .WithSummary("Retrieve membership type");

        var settings = app.MapGroup("/admin/settings/membership")
            .WithTags("Admin: Settings")
            .RequireAuthorization("AdminOnly");
        settings.MapGet("/", GetMembershipSettings)
            .WithName("GetMembershipSettings")
            .WithSummary("Retrieve global membership settings");
        settings.MapPut("/", UpdateMembershipSettings)
            .WithName("UpdateMembershipSettings")
            .WithSummary("Update global membership settings");

        var dashboard = app.MapGroup("/admin/dashboard")
            .WithTags("Admin: Dashboard")
            .RequireAuthorization("AdminOnly");
        dashboard.MapGet("/stats", GetDashboardStats)
            .WithName("GetDashboardStats")
            .WithSummary("Retrieve dashboard statistics");

        var udfFields = app.MapGroup("/admin/udffields")
            .WithTags("Admin: UDF Fields")
            .RequireAuthorization("AdminOnly");
        udfFields.MapGet("/", ListUdfDefinitions)
            .WithName("ListUdfDefinitions")
            .WithSummary("List user-defined field definitions");
        udfFields.MapGet("/values", ListAllMemberUdfValues)
            .WithName("ListAllMemberUdfValues")
            .WithSummary("List every member's user-defined field values");
        udfFields.MapGet("/{id}", GetUdfDefinition)
            .WithName("GetUdfDefinition")
            .WithSummary("Retrieve a user-defined field definition");
        udfFields.MapPost("/", CreateUdfDefinition)
            .WithName("CreateUdfDefinition")
            .WithSummary("Create a user-defined field definition");
        udfFields.MapPut("/{id}", UpdateUdfDefinition)
            .WithName("UpdateUdfDefinition")
            .WithSummary("Update a user-defined field definition");
        udfFields.MapDelete("/{id}", DeleteUdfDefinition)
            .WithName("DeleteUdfDefinition")
            .WithSummary("Delete a user-defined field definition");
        udfFields.MapPost("/{id}/apply-default", ApplyDefaultToAllMembers)
            .WithName("ApplyUdfDefaultToAllMembers")
            .WithSummary("Apply a field's default value to every member");

        var memberContacts = app.MapGroup("/admin/members/{memberId}/contacts")
            .WithTags("Admin: Member Contacts")
            .RequireAuthorization("AdminOnly");
        memberContacts.MapGet("/", ListMemberContacts)
            .WithName("ListMemberContacts")
            .WithSummary("List a member's contact details");
        memberContacts.MapPost("/", CreateMemberContact)
            .WithName("CreateMemberContact")
            .WithSummary("Add a contact detail for a member");
        memberContacts.MapPut("/{contactId}", UpdateMemberContact)
            .WithName("UpdateMemberContact")
            .WithSummary("Update a member's contact detail");
        memberContacts.MapDelete("/{contactId}", DeleteMemberContact)
            .WithName("DeleteMemberContact")
            .WithSummary("Delete a member's contact detail");

        var memberUdfValues = app.MapGroup("/admin/members/{memberId}/udf-values")
            .WithTags("Admin: Member UDF Values")
            .RequireAuthorization("AdminOnly");
        memberUdfValues.MapGet("/", ListMemberUdfValues)
            .WithName("ListMemberUdfValues")
            .WithSummary("List a member's user-defined field values");
        memberUdfValues.MapPut("/{definitionId}", UpdateMemberUdfValue)
            .WithName("UpdateMemberUdfValue")
            .WithSummary("Update a member's value for a user-defined field");

        var memberTokens = app.MapGroup("/admin/members/{memberId}/tokens")
            .WithTags("Admin: Member Tokens")
            .RequireAuthorization("AdminOnly");
        memberTokens.MapGet("/", ListMemberTokens)
            .WithName("ListMemberTokens")
            .WithSummary("List a member's tokens");
        memberTokens.MapPost("/", CreateMemberToken)
            .WithName("CreateMemberToken")
            .WithSummary("Assign a new token to a member");
        memberTokens.MapDelete("/{tokenId}", RevokeMemberToken)
            .WithName("RevokeMemberToken")
            .WithSummary("Revoke a member's token");

        var tokens = app.MapGroup("/admin/tokens")
            .WithTags("Admin: Tokens")
            .RequireAuthorization("AdminOnly");
        tokens.MapGet("/lookup", LookupToken)
            .WithName("LookupToken")
            .WithSummary("Look up the member owning a token (used by the in-app scan tool)");
    }


    private static async Task<Results<Ok<MembershipTypeDto>, NotFound>> GetMembershipType(
        [FromRoute] int id, [FromServices] GetMembershipTypeHandler handler, CancellationToken ct
        )
    {
        var membershipType = await handler.Handle(new GetMembershipTypeQuery(id), ct);
        if (membershipType is null) return TypedResults.NotFound();
        return TypedResults.Ok(membershipType);
    }
    private static async Task<Ok<List<MembershipTypeDto>>> ListMembershipTypes(ListMembershipTypeHandler hander, CancellationToken ct)
    {
        var types = await hander.Handle(ct);
        return TypedResults.Ok(types);
    }

    private static async Task<Results<Created<MembershipTypeDto>, ValidationProblem>> CreateMembershipType(
        CreateMembershipTypeRequest request, CreateMembershipTypeHandler handler, CancellationToken ct)
    {
        var result = await handler.Handle(request, ct);
        if (result.MembershipType is null)
            return TypedResults.ValidationProblem(new Dictionary<string, string[]> { ["renewalMode"] = [result.Error!] });

        return TypedResults.Created($"/admin/membershiptypes/{result.MembershipType.Id}", result.MembershipType);
    }

    private static async Task<Results<Created<MembershipDto>, NotFound, ValidationProblem>> CreateMembership(
        [FromRoute] int memberId, CreateMembershipRequest request, CreateMembershipHandler handler, CancellationToken ct)
    {
        var result = await handler.Handle(memberId, request, ct);
        return result.Status switch
        {
            CreateMembershipStatus.Success => TypedResults.Created(
                $"/admin/members/{memberId}/memberships/{result.Membership!.Id}", result.Membership),
            CreateMembershipStatus.MemberNotFound => TypedResults.NotFound(),
            CreateMembershipStatus.MembershipTypeNotFound => TypedResults.NotFound(),
            _ => TypedResults.ValidationProblem(new Dictionary<string, string[]> { ["membershipTypeId"] = [result.Error!] }),
        };
    }

    private static async Task<Ok<List<MembershipDto>>> ListMemberMemberships(
        [FromRoute] int memberId, ListMemberMembershipsHandler handler, CancellationToken ct)
    {
        var memberships = await handler.Handle(memberId, ct);
        return TypedResults.Ok(memberships);
    }

    private static async Task<Results<Ok<MembershipDto>, NotFound>> RenewMembership(
        [FromRoute] int memberId, [FromRoute] int membershipId, RenewMembershipHandler handler, CancellationToken ct)
    {
        var membership = await handler.Handle(memberId, membershipId, ct);
        if (membership is null) return TypedResults.NotFound();
        return TypedResults.Ok(membership);
    }

    private static async Task<Ok<MembershipSettingsDto>> GetMembershipSettings(
        GetMembershipSettingsHandler handler, CancellationToken ct)
    {
        var settings = await handler.Handle(ct);
        return TypedResults.Ok(settings);
    }

    private static async Task<Ok<MembershipSettingsDto>> UpdateMembershipSettings(
        UpdateMembershipSettingsRequest request, UpdateMembershipSettingsHandler handler, CancellationToken ct)
    {
        var settings = await handler.Handle(request, ct);
        return TypedResults.Ok(settings);
    }


    private static async Task<Ok<PagedResult<MemberDto>>> ListMembers(
        ListMemberHandler handler,
        CancellationToken ct,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25)
    {
        var members = await handler.Handle(page < 1 ? 1 : page, pageSize < 1 ? 25 : pageSize, ct);
        return TypedResults.Ok(members);

    }

    private static async Task<Ok<PagedResult<MemberDto>>> SearchMembers(
        [FromQuery] string q,
        [FromServices] SearchMemberHandler handler,
        CancellationToken ct,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25)
    {
        var members = await handler.Handle(new SearchMemberQuery(q), page < 1 ? 1 : page, pageSize < 1 ? 25 : pageSize, ct);

        return TypedResults.Ok(members);
    }

    private static async Task<Results<Ok<MemberDto>, NotFound>> GetMember(
    [FromRoute] int id, [FromServices] GetMemberHandler handler, CancellationToken ct)
    {
        var member = await handler.Handle(new GetMemberQuery(id), ct);

        if (member is null) return TypedResults.NotFound();

        return TypedResults.Ok(member);
    }

    private static async Task<Ok<DashboardStatsDto>> GetDashboardStats(
        GetDashboardStatsHandler handler, CancellationToken ct)
    {
        var stats = await handler.Handle(ct);
        return TypedResults.Ok(stats);
    }

    private static async Task<Results<Created<MemberDto>, ValidationProblem>> CreateMember(
        CreateMemberRequest request,
        CreateMemberHandler handler,
        CancellationToken ct)
    {
        var member = await handler.Handle(
            request, ct);

        return TypedResults.Created($"/admin/members/{member.Id}", member);
    }

    private static async Task<Ok<List<UdfDefinitionDto>>> ListUdfDefinitions(
        ListUdfDefinitionsHandler handler, CancellationToken ct)
    {
        var definitions = await handler.Handle(ct);
        return TypedResults.Ok(definitions);
    }

    private static async Task<Results<Ok<UdfDefinitionDto>, NotFound>> GetUdfDefinition(
        [FromRoute] int id, GetUdfDefinitionHandler handler, CancellationToken ct)
    {
        var definition = await handler.Handle(new GetUdfDefinitionQuery(id), ct);
        if (definition is null) return TypedResults.NotFound();
        return TypedResults.Ok(definition);
    }

    private static async Task<Results<Created<UdfDefinitionDto>, ValidationProblem>> CreateUdfDefinition(
        CreateUdfDefinitionRequest request, CreateUdfDefinitionHandler handler, CancellationToken ct)
    {
        var result = await handler.Handle(request, ct);
        if (result.Definition is null)
            return TypedResults.ValidationProblem(new Dictionary<string, string[]> { ["name"] = [result.Error!] });

        return TypedResults.Created($"/admin/udffields/{result.Definition.Id}", result.Definition);
    }

    private static async Task<Results<Ok<UdfDefinitionDto>, NotFound, ValidationProblem>> UpdateUdfDefinition(
        [FromRoute] int id, UpdateUdfDefinitionRequest request, UpdateUdfDefinitionHandler handler, CancellationToken ct)
    {
        var result = await handler.Handle(id, request, ct);
        if (result.NotFoundResult) return TypedResults.NotFound();
        if (result.Definition is null)
            return TypedResults.ValidationProblem(new Dictionary<string, string[]> { ["name"] = [result.Error!] });

        return TypedResults.Ok(result.Definition);
    }

    private static async Task<Results<NoContent, NotFound>> DeleteUdfDefinition(
        [FromRoute] int id, DeleteUdfDefinitionHandler handler, CancellationToken ct)
    {
        var deleted = await handler.Handle(id, ct);
        if (!deleted) return TypedResults.NotFound();
        return TypedResults.NoContent();
    }

    private static async Task<Results<Ok<UdfDefinitionDto>, NotFound>> ApplyDefaultToAllMembers(
        [FromRoute] int id, ApplyDefaultToAllMembersHandler handler, CancellationToken ct)
    {
        var definition = await handler.Handle(id, ct);
        if (definition is null) return TypedResults.NotFound();
        return TypedResults.Ok(definition);
    }

    private static async Task<Ok<UdfValuesGridDto>> ListAllMemberUdfValues(
        ListAllMemberUdfValuesHandler handler, CancellationToken ct)
    {
        var grid = await handler.Handle(ct);
        return TypedResults.Ok(grid);
    }

    private static async Task<Ok<List<MemberUdfFieldDto>>> ListMemberUdfValues(
        [FromRoute] int memberId, ListMemberUdfValuesHandler handler, CancellationToken ct)
    {
        var values = await handler.Handle(memberId, ct);
        return TypedResults.Ok(values);
    }

    private static async Task<Results<Ok<MemberUdfValueDto>, NotFound, ValidationProblem>> UpdateMemberUdfValue(
        [FromRoute] int memberId, [FromRoute] int definitionId, UpdateMemberUdfValueBody body,
        UpdateMemberUdfValueHandler handler, CancellationToken ct)
    {
        var result = await handler.Handle(memberId, definitionId, body.Value, ct);
        return result.Status switch
        {
            UpdateMemberUdfValueStatus.Success => TypedResults.Ok(result.Value!),
            UpdateMemberUdfValueStatus.MemberNotFound => TypedResults.NotFound(),
            UpdateMemberUdfValueStatus.DefinitionNotFound => TypedResults.NotFound(),
            _ => TypedResults.ValidationProblem(new Dictionary<string, string[]> { ["value"] = [result.Error!] }),
        };
    }

    private static async Task<Ok<List<ContactDto>>> ListMemberContacts(
        [FromRoute] int memberId, ListMemberContactsHandler handler, CancellationToken ct)
    {
        var contacts = await handler.Handle(memberId, ct);
        return TypedResults.Ok(contacts);
    }

    private static async Task<Results<Created<ContactDto>, NotFound, ValidationProblem>> CreateMemberContact(
        [FromRoute] int memberId, CreateMemberContactRequest request, CreateMemberContactHandler handler, CancellationToken ct)
    {
        var result = await handler.Handle(memberId, request, ct);
        return result.Status switch
        {
            CreateMemberContactStatus.Success => TypedResults.Created(
                $"/admin/members/{memberId}/contacts/{result.Contact!.Id}", result.Contact),
            CreateMemberContactStatus.MemberNotFound => TypedResults.NotFound(),
            _ => TypedResults.ValidationProblem(new Dictionary<string, string[]> { ["contactDetail"] = [result.Error!] }),
        };
    }

    private static async Task<Results<Ok<ContactDto>, NotFound, ValidationProblem>> UpdateMemberContact(
        [FromRoute] int memberId, [FromRoute] int contactId, UpdateMemberContactRequest request,
        UpdateMemberContactHandler handler, CancellationToken ct)
    {
        var result = await handler.Handle(memberId, contactId, request, ct);
        return result.Status switch
        {
            UpdateMemberContactStatus.Success => TypedResults.Ok(result.Contact!),
            UpdateMemberContactStatus.NotFound => TypedResults.NotFound(),
            _ => TypedResults.ValidationProblem(new Dictionary<string, string[]> { ["contactDetail"] = [result.Error!] }),
        };
    }

    private static async Task<Results<NoContent, NotFound>> DeleteMemberContact(
        [FromRoute] int memberId, [FromRoute] int contactId, DeleteMemberContactHandler handler, CancellationToken ct)
    {
        var deleted = await handler.Handle(memberId, contactId, ct);
        if (!deleted) return TypedResults.NotFound();
        return TypedResults.NoContent();
    }

    private static async Task<Ok<List<TokenDto>>> ListMemberTokens(
        [FromRoute] int memberId, ListMemberTokensHandler handler, CancellationToken ct)
    {
        var tokens = await handler.Handle(memberId, ct);
        return TypedResults.Ok(tokens);
    }

    private static async Task<Results<Created<TokenDto>, NotFound, ValidationProblem>> CreateMemberToken(
        [FromRoute] int memberId, CreateMemberTokenRequest request, CreateMemberTokenHandler handler, CancellationToken ct)
    {
        var result = await handler.Handle(memberId, request, ct);
        return result.Status switch
        {
            CreateMemberTokenStatus.Success => TypedResults.Created(
                $"/admin/members/{memberId}/tokens/{result.Token!.Id}", result.Token),
            CreateMemberTokenStatus.MemberNotFound => TypedResults.NotFound(),
            _ => TypedResults.ValidationProblem(new Dictionary<string, string[]> { ["value"] = [result.Error!] }),
        };
    }

    private static async Task<Results<NoContent, NotFound>> RevokeMemberToken(
        [FromRoute] int memberId, [FromRoute] int tokenId, RevokeMemberTokenHandler handler, CancellationToken ct)
    {
        var revoked = await handler.Handle(memberId, tokenId, ct);
        if (!revoked) return TypedResults.NotFound();
        return TypedResults.NoContent();
    }

    private static async Task<Results<Ok<TokenLookupDto>, NotFound>> LookupToken(
        [FromQuery] string value, LookupTokenHandler handler, CancellationToken ct)
    {
        var result = await handler.Handle(value, ct);
        if (result is null) return TypedResults.NotFound();
        return TypedResults.Ok(result);
    }
}

internal sealed record UpdateMemberUdfValueBody(string? Value);


