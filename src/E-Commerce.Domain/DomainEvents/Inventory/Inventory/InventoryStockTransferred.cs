using System;

namespace E_Commerce.Domain.DomainEvents.Inventory.Inventory
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