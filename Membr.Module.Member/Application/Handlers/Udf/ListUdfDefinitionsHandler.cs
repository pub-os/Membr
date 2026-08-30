namespace Membr.Module.Member.Application.Handlers.Udf;

using Microsoft.EntityFrameworkCore;
using Persistence;

internal sealed class ListUdfDefinitionsHandler(MembersDbContext db)
{
    public async Task<List<UdfDefinitionDto>> Handle(CancellationToken ct)
    {
        var definitions = await db.UdfDefinitions.ToListAsync(ct);
        return [.. definitions.Select(UdfDefinitionDto.FromEntity)];
    }
}
