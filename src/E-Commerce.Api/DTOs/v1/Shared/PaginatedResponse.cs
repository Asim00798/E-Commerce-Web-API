namespace E_Commerce.Api.DTOs.v1.Shared;

/// <summary>
/// API-level envelope for paginated responses.
/// Preserves the full pagination metadata (total count, page numbers,
/// navigation flags) that a plain list would discard, so clients can
/// render pagination controls correctly.
/// </summary>
public sealed class PaginatedResponse<T>
{
    /// <summary>The items on the current page.</summary>
    public IReadOnlyList<T> Items { get; init; } = [];

    /// <summary>Current page number (1-based).</summary>
    public int PageNumber { get; init; }

    /// <summary>Number of items per page.</summary>
    public int PageSize { get; init; }

    /// <summary>Total number of pages.</summary>
    public int TotalPages { get; init; }

    /// <summary>Total number of items across all pages.</summary>
    public int TotalCount { get; init; }

    /// <summary>True when a previous page exists.</summary>
    public bool HasPreviousPage { get; init; }

    /// <summary>True when a next page exists.</summary>
    public bool HasNextPage { get; init; }
}