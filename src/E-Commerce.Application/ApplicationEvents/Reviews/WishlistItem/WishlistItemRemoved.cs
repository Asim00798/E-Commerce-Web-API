using E_Commerce.Domain.DomainEvents;
using System;

namespace E_Commerce.Application.ApplicationEvents.Reviews.WishlistItem
{
    public sealed class WishlistItemRemoved : DomainEvent
    {
        public Guid AggregateId { get; }

        public WishlistItemRemoved(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}