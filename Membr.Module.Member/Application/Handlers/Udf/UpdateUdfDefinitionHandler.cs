namespace Membr.Module.Member.Application.Handlers.Udf;

using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Persistence;

internal sealed class UpdateUdfDefinitionHandler(MembersDbContext db)
{
    public async Task<UpdateUdfDefinitionResult> Handle(int id, UpdateUdfDefinitionRequest request, CancellationToken ct)
    {
        var definition = await db.UdfDefinitions.FindAsync([id], ct);
        if (definition is null)
            return UpdateUdfDefinitionResult.NotFound();

        var options = request.Options ?? [];
        var validationError = UdfValueValidation.ValidateDefinition(request.Name, definition.Type, options, request.DefaultValue);
        if (validationError is not null)
            return UpdateUdfDefinitionResult.Invalid(validationError);

        if (await db.UdfDefinitions.AnyAsync(d => d.Id != id && d.Name == request.Name, ct))
            return UpdateUdfDefinitionResult.Invalid("A field with this name already exists.");

        var removedOptions = definition.Options.Except(options).ToList();

        definition.Name = request.Name;
        definition.Options = options;
        definition.DefaultValue = request.DefaultValue;
        definition.IsActive = request.IsActive;

        if (definition.Type == Domain.UdfFieldType.MultiSelect && removedOptions.Count > 0)
        {
            var values = await db.MemberUdfValues
                .Where(v => v.UdfDefinitionId == id && v.Value != null)
                .ToListAsync(ct);

            foreach (var value in values)
            {
                var selected = JsonSerializer.Deserialize<List<string>>(value.Value!) ?? [];
                var filtered = selected.Where(s => !removedOptions.Contains(s)).ToList();
                if (filtered.Count != selected.Count)
                    value.Value = JsonSerializer.Serialize(filtered);
            }
        }

        await db.SaveChangesAsync(ct);
        return UpdateUdfDefinitionResult.Success(UdfDefinitionDto.FromEntity(definition));
    }
}

internal sealed record UpdateUdfDefinitionResult(UdfDefinitionDto? Definition, string? Error, bool NotFoundResult)
{
    public static UpdateUdfDefinitionResult Success(UdfDefinitionDto dto) => new(dto, null, false);
    public static UpdateUdfDefinitionResult Invalid(string error) => new(null, error, false);
    public static UpdateUdfDefinitionResult NotFound() => new(null, null, true);
}

internal sealed record UpdateUdfDefinitionRequest(
    string Name,
    List<string>? Options,
    string? DefaultValue,
    bool IsActive);
