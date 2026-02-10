using System;

namespace E_Commerce.Domain.DomainEvents.Shipping.ShipmentItem
{
    public sealed class ShipmentItemDamaged : DomainEvent
    {
        public Guid AggregateId { get; }

        public ShipmentItemDamaged(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}