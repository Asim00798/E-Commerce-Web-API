namespace E_Commerce.Api.DTOs.CustomerEngagement.Responses;

public sealed class ProductRatingsSummaryResponse
{
    public Guid ProductId { get; init; }
    public double AverageRating { get; init; }
    public int TotalCount { get; init; }
    public IReadOnlyDictionary<int, int> Distribution { get; init; } = new Dictionary<int, int>();
}