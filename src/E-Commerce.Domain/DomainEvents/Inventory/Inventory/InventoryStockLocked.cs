using System;

namespace E_Commerce.Domain.DomainEvents.Inventory.Inventory
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