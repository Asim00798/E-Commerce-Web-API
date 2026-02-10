using E_Commerce.Domain.DomainEvents;
using System;

namespace E_Commerce.Application.ApplicationEvents.Reviews.WishlistItem
{
    public sealed class WishlistItemPriorityChanged : DomainEvent
    {
        public Guid AggregateId { get; }

        public WishlistItemPriorityChanged(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}