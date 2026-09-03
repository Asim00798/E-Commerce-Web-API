using E_Commerce.Domain.BoundedContexts.Core.CustomerEngagement.AggregateRoots.Rating.Exceptions;

namespace E_Commerce.Domain.BoundedContexts.Core.CustomerEngagement.AggregateRoots.Rating.Behaviors;

public sealed partial class Rating
{
    public override void Validate()
    {
        base.Validate();

        if (CustomerId == Guid.Empty)
            throw new RatingException("Customer ID is required.");

        if (ProductId == Guid.Empty)
            throw new RatingException("Product ID is required.");

        if (StarRating is null)
            throw new RatingException("Star rating is required.");
    }
}