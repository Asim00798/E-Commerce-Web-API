using System;

namespace E_Commerce.Domain.DomainEvents.Inventory.Warehouse
{
    public sealed class WarehouseDeactivated : DomainEvent
    {
        public Guid AggregateId { get; }

        public WarehouseDeactivated(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}