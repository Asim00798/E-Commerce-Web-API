using E_Commerce.Domain.DomainEvents;
using System;

namespace E_Commerce.Application.ApplicationEvents.Reviews.WishlistItem
{
    public sealed class WishlistItemAdded : DomainEvent
    {
        public Guid AggregateId { get; }

        public WishlistItemAdded(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}