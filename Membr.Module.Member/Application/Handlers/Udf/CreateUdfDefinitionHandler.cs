namespace Membr.Module.Member.Application.Handlers.Udf;

using Domain;
using Microsoft.EntityFrameworkCore;
using Persistence;

internal sealed class CreateUdfDefinitionHandler(MembersDbContext db)
{
    public async Task<CreateUdfDefinitionResult> Handle(CreateUdfDefinitionRequest request, CancellationToken ct)
    {
        var options = request.Options ?? [];
        var validationError = UdfValueValidation.ValidateDefinition(request.Name, request.Type, options, request.DefaultValue);
        if (validationError is not null)
            return CreateUdfDefinitionResult.Invalid(validationError);

        if (await db.UdfDefinitions.AnyAsync(d => d.Name == request.Name, ct))
            return CreateUdfDefinitionResult.Invalid("A field with this name already exists.");

        var definition = new UdfDefinition
        {
            Name = request.Name,
            Type = request.Type,
            IsActive = true,
            Options = options,
            DefaultValue = request.DefaultValue,
        };

        db.UdfDefinitions.Add(definition);

        var memberIds = await db.Members.Select(m => m.Id).ToListAsync(ct);
        foreach (var memberId in memberIds)
        {
            db.MemberUdfValues.Add(new MemberUdfValue
            {
                MemberId = memberId,
                UdfDefinition = definition,
                Value = request.DefaultValue,
            });
        }

        await db.SaveChangesAsync(ct);
        return CreateUdfDefinitionResult.Success(UdfDefinitionDto.FromEntity(definition));
    }
}

internal sealed record CreateUdfDefinitionResult(UdfDefinitionDto? Definition, string? Error)
{
    public static CreateUdfDefinitionResult Success(UdfDefinitionDto dto) => new(dto, null);
    public static CreateUdfDefinitionResult Invalid(string error) => new(null, error);
}

internal sealed record CreateUdfDefinitionRequest(
    string Name,
    UdfFieldType Type,
    List<string>? Options,
    string? DefaultValue);
