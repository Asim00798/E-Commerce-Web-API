namespace E_Commerce.ReadModel.Abstractions;

/// <summary>
/// Represents a paginated result set.
/// </summary>
/// <typeparam name="T">The item type.</typeparam>
public interface IPagedResult<T>
{
    IReadOnlyList<T> Items { get; }
    int TotalCount { get; }
    int PageNumber { get; }
    int PageSize { get; }
    int TotalPages { get; }
    bool HasPreviousPage { get; }
    bool HasNextPage { get; }
}
