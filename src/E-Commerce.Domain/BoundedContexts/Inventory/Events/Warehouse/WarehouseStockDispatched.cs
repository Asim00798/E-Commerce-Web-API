using System;

namespace E_Commerce.Domain.BoundedContexts.Inventory.Inventory.Warehouse
{
    public sealed class WarehouseStockDispatched : DomainEvent
    {
        public Guid AggregateId { get; }

        public WarehouseStockDispatched(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}