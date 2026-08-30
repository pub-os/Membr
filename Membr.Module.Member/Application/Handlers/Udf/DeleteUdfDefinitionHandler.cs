namespace Membr.Module.Member.Application.Handlers.Udf;

using Persistence;

internal sealed class DeleteUdfDefinitionHandler(MembersDbContext db)
{
    public async Task<bool> Handle(int id, CancellationToken ct)
    {
        var definition = await db.UdfDefinitions.FindAsync([id], ct);
        if (definition is null)
            return false;

        db.UdfDefinitions.Remove(definition);
        await db.SaveChangesAsync(ct);
        return true;
    }
}
