using System;

namespace E_Commerce.Domain.DomainEvents.Reviews.Wishlist
{
    public sealed class WishlistArchived : DomainEvent
    {
        public Guid AggregateId { get; }

        public WishlistArchived(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}