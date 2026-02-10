using E_Commerce.Domain.DomainEvents;
using System;

namespace E_Commerce.Application.ApplicationEvents.Reviews.Wishlist
{
    public sealed class WishlistRenamed : DomainEvent
    {
        public Guid AggregateId { get; }

        public WishlistRenamed(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}