using System;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Ordering.Ordering.Cart
{
    public sealed class CartCreated : DomainEvent
    {
        public Guid AggregateId { get; }

        public CartCreated(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}