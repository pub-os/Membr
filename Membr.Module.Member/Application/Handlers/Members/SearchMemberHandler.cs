namespace Membr.Module.Member.Application.Handlers.Members;

using Membr.Shared;
using Persistence;
using Microsoft.EntityFrameworkCore;



internal sealed class SearchMemberHandler(MembersDbContext db)
{
    public async Task<PagedResult<MemberDto>> Handle(SearchMemberQuery query, int page, int pageSize, CancellationToken ct)
    {
        var terms = query.SearchText.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var membersQuery = db.Members.AsNoTracking();

        // Each word must match either name field, so "John Smith" (or "Smith John") requires
        // one word to hit FirstName and the other to hit Surname, in any order.
        foreach (var term in terms)
        {
            var pattern = $"%{term}%";
            membersQuery = membersQuery.Where(m =>
                EF.Functions.ILike(m.FirstName, pattern) ||
                EF.Functions.ILike(m.Surname, pattern));
        }

        membersQuery = membersQuery
            .OrderBy(m => m.Surname)
            .ThenBy(m => m.FirstName);

        var totalCount = await membersQuery.CountAsync(ct);
        var members = await membersQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<MemberDto>([.. members.Select(MemberDto.FromEntity)], totalCount, page, pageSize);
    }
}
internal sealed record SearchMemberQuery(
    string SearchText
);
