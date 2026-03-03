using System;

namespace E_Commerce.Domain.BoundedContexts.Ordering.Ordering.Cart
{
    public sealed class CartCleared : DomainEvent
    {
        public Guid AggregateId { get; }

        public CartCleared(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}