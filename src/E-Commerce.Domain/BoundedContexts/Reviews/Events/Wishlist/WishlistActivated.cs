using System;

namespace E_Commerce.Domain.BoundedContexts.Reviews.Reviews.Wishlist
{
    public sealed class WishlistActivated : DomainEvent
    {
        public Guid AggregateId { get; }

        public WishlistActivated(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}