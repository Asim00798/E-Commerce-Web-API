using E_Commerce.Domain.DomainEvents;
using System;

namespace E_Commerce.Application.ApplicationEvents.Reviews.Wishlist
{
    public sealed class WishlistShared : DomainEvent
    {
        public Guid AggregateId { get; }

        public WishlistShared(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}