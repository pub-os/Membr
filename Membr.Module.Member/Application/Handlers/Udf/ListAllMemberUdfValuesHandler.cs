namespace Membr.Module.Member.Application.Handlers.Udf;

using Microsoft.EntityFrameworkCore;
using Persistence;

internal sealed class ListAllMemberUdfValuesHandler(MembersDbContext db)
{
    public async Task<UdfValuesGridDto> Handle(CancellationToken ct)
    {
        var definitions = await db.UdfDefinitions.ToListAsync(ct);
        var members = await db.Members.ToListAsync(ct);
        var values = await db.MemberUdfValues.ToListAsync(ct);

        return new UdfValuesGridDto(
            [.. definitions.Select(UdfDefinitionDto.FromEntity)],
            [.. members.Select(MemberSummaryDto.FromEntity)],
            [.. values.Select(MemberUdfValueDto.FromEntity)]);
    }
}
