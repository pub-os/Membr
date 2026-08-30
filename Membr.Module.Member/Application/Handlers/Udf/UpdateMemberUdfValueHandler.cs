namespace Membr.Module.Member.Application.Handlers.Udf;

using Domain;
using Microsoft.EntityFrameworkCore;
using Persistence;

internal sealed class UpdateMemberUdfValueHandler(MembersDbContext db)
{
    public async Task<UpdateMemberUdfValueResult> Handle(int memberId, int definitionId, string? value, CancellationToken ct)
    {
        var member = await db.Members.FindAsync([memberId], ct);
        if (member is null)
            return UpdateMemberUdfValueResult.MemberNotFound();

        var definition = await db.UdfDefinitions.FindAsync([definitionId], ct);
        if (definition is null)
            return UpdateMemberUdfValueResult.DefinitionNotFound();

        var validationError = UdfValueValidation.ValidateValue(definition.Type, value, definition.Options);
        if (validationError is not null)
            return UpdateMemberUdfValueResult.Invalid(validationError);

        var existing = await db.MemberUdfValues
            .FirstOrDefaultAsync(v => v.MemberId == memberId && v.UdfDefinitionId == definitionId, ct);

        if (existing is null)
        {
            existing = new MemberUdfValue { MemberId = memberId, UdfDefinitionId = definitionId };
            db.MemberUdfValues.Add(existing);
        }

        existing.Value = value;
        await db.SaveChangesAsync(ct);

        return UpdateMemberUdfValueResult.Success(MemberUdfValueDto.FromEntity(existing));
    }
}

internal enum UpdateMemberUdfValueStatus
{
    Success,
    MemberNotFound,
    DefinitionNotFound,
    Invalid,
}

internal sealed record UpdateMemberUdfValueResult(UpdateMemberUdfValueStatus Status, MemberUdfValueDto? Value, string? Error)
{
    public static UpdateMemberUdfValueResult Success(MemberUdfValueDto dto) => new(UpdateMemberUdfValueStatus.Success, dto, null);
    public static UpdateMemberUdfValueResult MemberNotFound() => new(UpdateMemberUdfValueStatus.MemberNotFound, null, null);
    public static UpdateMemberUdfValueResult DefinitionNotFound() => new(UpdateMemberUdfValueStatus.DefinitionNotFound, null, null);
    public static UpdateMemberUdfValueResult Invalid(string error) => new(UpdateMemberUdfValueStatus.Invalid, null, error);
}
