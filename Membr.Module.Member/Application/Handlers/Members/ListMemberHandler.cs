namespace Membr.Module.Member.Application.Handlers.Members;

using Membr.Shared;
using Persistence;
using Microsoft.EntityFrameworkCore;

internal sealed class ListMemberHandler(MembersDbContext dbContext)
{
    public async Task<PagedResult<MemberDto>> Handle(int page, int pageSize, CancellationToken ct)
    {
        var query = dbContext.Members
            .OrderBy(m => m.Surname)
            .ThenBy(m => m.FirstName);

        var totalCount = await query.CountAsync(ct);
        var members = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<MemberDto>([.. members.Select(MemberDto.FromEntity)], totalCount, page, pageSize);
    }
}
