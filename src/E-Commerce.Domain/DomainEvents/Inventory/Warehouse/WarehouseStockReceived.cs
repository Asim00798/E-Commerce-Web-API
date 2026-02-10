using System;

namespace E_Commerce.Domain.DomainEvents.Inventory.Warehouse
{
    public sealed class WarehouseStockReceived : DomainEvent
    {
        public Guid AggregateId { get; }

        public WarehouseStockReceived(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}