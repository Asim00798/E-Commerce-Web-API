using System;

namespace E_Commerce.Domain.DomainEvents.Reviews.Wishlist
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