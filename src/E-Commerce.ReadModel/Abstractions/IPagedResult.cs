namespace E_Commerce.ReadModel.Abstractions;

public interface IPagedResult<out T>
{
    IEnumerable<T> Items { get; }
    int PageNumber { get; }
    int PageSize { get; }
    int TotalCount { get; }
    int TotalPages { get; }
    bool HasPreviousPage { get; }
    bool HasNextPage { get; }
}
