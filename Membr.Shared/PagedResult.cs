namespace Membr.Shared;

public sealed record PagedResult<T>(List<T> Items, int TotalCount, int Page, int PageSize);
