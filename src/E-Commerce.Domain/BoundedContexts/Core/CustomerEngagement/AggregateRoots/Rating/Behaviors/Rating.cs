using E_Commerce.Domain.BoundedContexts.Core.CustomerEngagement.AggregateRoots.Rating.Exceptions;
using E_Commerce.Domain.BoundedContexts.Core.CustomerEngagement.AggregateRoots.Rating.ValueObjects;
using E_Commerce.Domain.SharedKernel.Abstractions;

namespace E_Commerce.Domain.BoundedContexts.Core.CustomerEngagement.AggregateRoots.Rating.Behaviors;

/// <summary>
/// Represents a customer's star rating for a product.
/// Aggregate root that enforces a single active rating per customer per product.
/// </summary>
public sealed partial class Rating : BaseEntity, IAggregateRoot
{
    public Guid CustomerId { get; private set; }
    public Guid ProductId { get; private set; }
    public StarRating StarRating { get; private set; } = null!;
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }

    private Rating()
    {
        // EF Core
    }

    private Rating(
        Guid customerId,
        Guid productId,
        StarRating starRating)
    {
        CustomerId = customerId;
        ProductId = productId;
        StarRating = starRating;
        CreatedAtUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Factory method to create a new rating.
    /// Validates inputs and raises RatingSubmitted event.
    /// </summary>
    public static Rating Create(
        Guid customerId,
        Guid productId,
        StarRating starRating)
    {
        if (customerId == Guid.Empty)
            throw new RatingException("Customer ID is required.");

        if (productId == Guid.Empty)
            throw new RatingException("Product ID is required.");

        if (starRating is null)
            throw new RatingException("Star rating is required.");

        return new Rating(customerId, productId, starRating);
    }

    /// <summary>
    /// Updates the star rating value.
    /// Replaces the current value object and raises RatingUpdated event.
    /// </summary>
    public void UpdateStarRating(StarRating newStarRating)
    {
        if (newStarRating is null)
            throw new RatingException("Star rating is required.");

        StarRating = newStarRating;
        UpdatedAtUtc = DateTime.UtcNow;

    }
}