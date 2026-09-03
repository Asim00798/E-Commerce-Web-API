namespace E_Commerce.Application.BoundedContexts.CustomerEngagement.Ratings.DTOs;

/// <summary>
/// Summary of ratings for a product: average, total count, and distribution.
/// </summary>
public sealed class ProductRatingsSummaryDto
{
    public Guid ProductId { get; init; }
    public double AverageRating { get; init; }
    public int TotalCount { get; init; }
    public IReadOnlyDictionary<int, int> Distribution { get; init; } = new Dictionary<int, int>();
}