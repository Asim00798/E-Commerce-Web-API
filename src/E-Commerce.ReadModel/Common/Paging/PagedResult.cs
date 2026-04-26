using E_Commerce.ReadModel.Abstractions;

namespace E_Commerce.ReadModel.Common.Paging;

/// <summary>
/// Concrete paginated result implementation.
/// </summary>
/// <typeparam name="T">The item type.</typeparam>
public sealed class PagedResult<T> : IPagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = [];
    public int TotalCount { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}
