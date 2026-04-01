using System;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Reviews.Reviews.Wishlist
{
    public sealed class WishlistCreated : DomainEvent
    {
        public Guid AggregateId { get; }

        public WishlistCreated(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}