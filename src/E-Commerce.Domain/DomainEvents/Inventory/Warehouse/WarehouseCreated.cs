using System;

namespace E_Commerce.Domain.DomainEvents.Inventory.Warehouse
{
    public sealed class WarehouseCreated : DomainEvent
    {
        public Guid AggregateId { get; }

        public WarehouseCreated(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}