using System;

namespace E_Commerce.Domain.BoundedContexts.Inventory.Inventory.Inventory
{
    public sealed class InventoryStockTransferred : DomainEvent
    {
        public Guid AggregateId { get; }

        public InventoryStockTransferred(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}