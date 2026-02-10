using E_Commerce.Domain.DomainEvents;
using System;

namespace E_Commerce.Application.ApplicationEvents.Reviews.Wishlist
{
    public sealed class WishlistCleared : DomainEvent
    {
        public Guid AggregateId { get; }

        public WishlistCleared(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}