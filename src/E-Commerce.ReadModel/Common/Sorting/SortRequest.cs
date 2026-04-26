namespace E_Commerce.ReadModel.Common.Sorting;

/// <summary>
/// Encapsulates sort parameters for a read query.
/// </summary>
public sealed class SortRequest
{
    public string? SortBy { get; init; }
    public bool Descending { get; init; }
}
