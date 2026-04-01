using System;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Inventory.Inventory.Inventory
{
    public sealed class InventoryStockLocked : DomainEvent
    {
        public Guid AggregateId { get; }

        public InventoryStockLocked(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}