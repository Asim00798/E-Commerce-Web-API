using Microsoft.EntityFrameworkCore;

namespace E_Commerce.ReadModel.Common.Paging;

public static class PagingExtensions
{
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(this IQueryable<T> source, int pageNumber, int pageSize)
    {
        return await PagedResult<T>.CreateAsync(source, pageNumber, pageSize);
    }
}
