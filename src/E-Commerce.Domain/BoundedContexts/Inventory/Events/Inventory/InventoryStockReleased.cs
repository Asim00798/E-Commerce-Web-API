using System;

namespace E_Commerce.Domain.BoundedContexts.Inventory.Inventory.Inventory
{
    public sealed class InventoryStockReleased : DomainEvent
    {
        public Guid AggregateId { get; }

        public InventoryStockReleased(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}