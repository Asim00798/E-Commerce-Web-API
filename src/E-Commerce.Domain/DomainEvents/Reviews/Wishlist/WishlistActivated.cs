using System;

namespace E_Commerce.Domain.DomainEvents.Reviews.Wishlist
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