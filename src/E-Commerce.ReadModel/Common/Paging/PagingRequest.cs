namespace E_Commerce.ReadModel.Common.Paging;

/// <summary>
/// Encapsulates paging parameters for a read query.
/// </summary>
public sealed class PagingRequest
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
