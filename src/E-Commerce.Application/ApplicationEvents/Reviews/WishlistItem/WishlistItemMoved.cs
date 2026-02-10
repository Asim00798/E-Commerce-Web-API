using E_Commerce.Domain.DomainEvents;
using System;

namespace E_Commerce.Application.ApplicationEvents.Reviews.WishlistItem
{
    public sealed class WishlistItemMoved : DomainEvent
    {
        public Guid AggregateId { get; }

        public WishlistItemMoved(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}