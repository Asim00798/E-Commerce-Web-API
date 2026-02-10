using E_Commerce.Domain.DomainEvents;
using System;

namespace E_Commerce.Application.ApplicationEvents.Reviews.WishlistItem
{
    public sealed class WishlistItemPriceDropNotified : DomainEvent
    {
        public Guid AggregateId { get; }

        public WishlistItemPriceDropNotified(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}