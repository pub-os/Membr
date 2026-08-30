namespace Membr.Module.Member.Application.Handlers.MembershipTypes;

using Persistence;
using Domain;

internal sealed class CreateMembershipTypeHandler(MembersDbContext db)
{
    public async Task<CreateMembershipTypeResult> Handle(CreateMembershipTypeRequest request, CancellationToken ct)
    {
        var validationError = MembershipTypeValidation.Validate(
            request.RenewalMode, request.DurationMonths, request.FixedTermAnchorMonth, request.FixedTermAnchorDay);

        if (validationError is not null)
            return CreateMembershipTypeResult.Invalid(validationError);

        var membershipType = new MembershipType
        {
            Name = request.Name,
            Description = request.Description,
            IsActive = request.IsActive,
            RenewalMode = request.RenewalMode,
            DurationMonths = request.DurationMonths,
            FixedTermAnchorMonth = request.FixedTermAnchorMonth,
            FixedTermAnchorDay = request.FixedTermAnchorDay,
        };

        db.MembershipTypes.Add(membershipType);
        await db.SaveChangesAsync(ct);
        return CreateMembershipTypeResult.Success(MembershipTypeDto.FromEntity(membershipType));
    }
}

internal sealed record CreateMembershipTypeResult(MembershipTypeDto? MembershipType, string? Error)
{
    public static CreateMembershipTypeResult Success(MembershipTypeDto dto) => new(dto, null);
    public static CreateMembershipTypeResult Invalid(string error) => new(null, error);
}

internal static class MembershipTypeValidation
{
    public static string? Validate(MembershipRenewalMode mode, int? durationMonths, int? anchorMonth, int? anchorDay)
    {
        switch (mode)
        {
            case MembershipRenewalMode.Rolling:
                if (durationMonths is not > 0)
                    return "DurationMonths must be a positive number of months for a rolling membership type.";
                if (anchorMonth is not null || anchorDay is not null)
                    return "Anchor month/day must not be set for a rolling membership type.";
                break;

            case MembershipRenewalMode.FixedTerm:
                if (anchorMonth is not (>= 1 and <= 12))
                    return "FixedTermAnchorMonth must be between 1 and 12 for a fixed-term membership type.";
                if (anchorDay is not (>= 1 and <= 31))
                    return "FixedTermAnchorDay must be between 1 and 31 for a fixed-term membership type.";
                if (durationMonths is not null)
                    return "DurationMonths must not be set for a fixed-term membership type.";
                break;

            default:
                return "Unknown renewal mode.";
        }

        return null;
    }
}

internal sealed record CreateMembershipTypeRequest(
    string Name,
    string? Description,
    bool IsActive,
    MembershipRenewalMode RenewalMode,
    int? DurationMonths,
    int? FixedTermAnchorMonth,
    int? FixedTermAnchorDay);
