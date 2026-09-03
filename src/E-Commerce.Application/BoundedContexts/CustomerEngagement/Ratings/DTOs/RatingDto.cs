namespace E_Commerce.Application.BoundedContexts.CustomerEngagement.Ratings.DTOs;

/// <summary>
/// Represents a single customer rating for a product.
/// </summary>
public sealed class RatingDto
{
    public Guid Id { get; init; }
    public Guid CustomerId { get; init; }
    public Guid ProductId { get; init; }
    public int StarRating { get; init; }
    public DateTime CreatedAtUtc { get; init; }
    public DateTime? UpdatedAtUtc { get; init; }
}