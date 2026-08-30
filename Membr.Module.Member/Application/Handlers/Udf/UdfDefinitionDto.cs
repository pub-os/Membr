namespace Membr.Module.Member.Application.Handlers.Udf;

using Domain;

internal sealed record UdfDefinitionDto(
    int Id,
    string Name,
    UdfFieldType Type,
    bool IsActive,
    List<string> Options,
    string? DefaultValue)
{
    public static UdfDefinitionDto FromEntity(UdfDefinition d) => new(
        d.Id, d.Name, d.Type, d.IsActive, d.Options, d.DefaultValue);
}
