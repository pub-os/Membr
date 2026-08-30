namespace Membr.Module.Member.Application.Handlers.Udf;

using Microsoft.EntityFrameworkCore;
using Persistence;

internal sealed class ListMemberUdfValuesHandler(MembersDbContext db)
{
    public async Task<List<MemberUdfFieldDto>> Handle(int memberId, CancellationToken ct)
    {
        var definitions = await db.UdfDefinitions
            .Where(d => d.IsActive)
            .ToListAsync(ct);

        var values = await db.MemberUdfValues
            .Where(v => v.MemberId == memberId)
            .ToListAsync(ct);

        var valuesByDefinitionId = values.ToDictionary(v => v.UdfDefinitionId, v => v.Value);

        return [.. definitions.Select(d => new MemberUdfFieldDto(
            d.Id,
            d.Name,
            d.Type,
            d.Options,
            valuesByDefinitionId.GetValueOrDefault(d.Id)))];
    }
}
