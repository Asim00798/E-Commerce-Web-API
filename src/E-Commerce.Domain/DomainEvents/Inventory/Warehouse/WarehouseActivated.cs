using System;

namespace E_Commerce.Domain.DomainEvents.Inventory.Warehouse
{
    public sealed class WarehouseActivated : DomainEvent
    {
        public Guid AggregateId { get; }

        public WarehouseActivated(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}