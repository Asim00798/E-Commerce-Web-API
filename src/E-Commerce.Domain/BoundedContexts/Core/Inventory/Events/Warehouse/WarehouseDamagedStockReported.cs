using System;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Inventory.Inventory.Warehouse
{
    public sealed class WarehouseDamagedStockReported : DomainEvent
    {
        public Guid AggregateId { get; }

        public WarehouseDamagedStockReported(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}