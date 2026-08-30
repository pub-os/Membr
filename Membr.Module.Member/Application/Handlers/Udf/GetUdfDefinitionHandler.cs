namespace Membr.Module.Member.Application.Handlers.Udf;

using Persistence;

internal sealed class GetUdfDefinitionHandler(MembersDbContext db)
{
    public async Task<UdfDefinitionDto?> Handle(GetUdfDefinitionQuery query, CancellationToken ct)
    {
        var definition = await db.UdfDefinitions.FindAsync([query.Id], ct);
        return definition is null ? null : UdfDefinitionDto.FromEntity(definition);
    }
}

internal sealed record GetUdfDefinitionQuery(int Id);
