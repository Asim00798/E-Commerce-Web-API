using E_Commerce.Domain.BoundedContexts.Core.CustomerEngagement.AggregateRoots.Wishlist.Exceptions;

namespace E_Commerce.Domain.BoundedContexts.Core.CustomerEngagement.AggregateRoots.Wishlist.Behaviors;

public sealed partial class Wishlist
{
    public override void Validate()
    {
        base.Validate();

        if (CustomerId == Guid.Empty)
            throw new WishlistException("Customer ID is required.");
    }
}