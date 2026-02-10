using System;

namespace E_Commerce.Domain.DomainEvents.Ordering.Cart
{
    public sealed class CartExpired : DomainEvent
    {
        public Guid AggregateId { get; }

        public CartExpired(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}