namespace Membr.Module.Member.Application.Handlers.Udf;

using Domain;
using Microsoft.EntityFrameworkCore;
using Persistence;

internal sealed class ApplyDefaultToAllMembersHandler(MembersDbContext db)
{
    public async Task<UdfDefinitionDto?> Handle(int definitionId, CancellationToken ct)
    {
        var definition = await db.UdfDefinitions.FindAsync([definitionId], ct);
        if (definition is null)
            return null;

        var existingValues = await db.MemberUdfValues
            .Where(v => v.UdfDefinitionId == definitionId)
            .ToListAsync(ct);

        foreach (var value in existingValues)
            value.Value = definition.DefaultValue;

        var memberIdsWithValues = existingValues.Select(v => v.MemberId).ToHashSet();
        var missingMemberIds = await db.Members
            .Where(m => !memberIdsWithValues.Contains(m.Id))
            .Select(m => m.Id)
            .ToListAsync(ct);

        foreach (var memberId in missingMemberIds)
        {
            db.MemberUdfValues.Add(new MemberUdfValue
            {
                MemberId = memberId,
                UdfDefinitionId = definitionId,
                Value = definition.DefaultValue,
            });
        }

        await db.SaveChangesAsync(ct);
        return UdfDefinitionDto.FromEntity(definition);
    }
}
