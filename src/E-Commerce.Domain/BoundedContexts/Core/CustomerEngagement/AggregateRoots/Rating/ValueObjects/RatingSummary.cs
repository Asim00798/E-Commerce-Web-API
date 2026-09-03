namespace E_Commerce.Domain.BoundedContexts.Core.CustomerEngagement.AggregateRoots.Rating.ValueObjects;

/// <summary>
/// Read model representing aggregate rating information for a product.
/// It is not an entity; it is produced by a query.
/// </summary>
public sealed record RatingSummary(
    double AverageRating,
    int TotalCount,
    IReadOnlyDictionary<int, int> Distribution);