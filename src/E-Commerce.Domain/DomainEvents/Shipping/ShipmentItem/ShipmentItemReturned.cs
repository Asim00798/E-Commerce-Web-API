using System;

namespace E_Commerce.Domain.DomainEvents.Shipping.ShipmentItem
{
    public sealed class ShipmentItemReturned : DomainEvent
    {
        public Guid AggregateId { get; }

        public ShipmentItemReturned(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}