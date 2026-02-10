using System;

namespace E_Commerce.Domain.DomainEvents.Inventory.Inventory
{
    public sealed class InventoryReorderLevelReached : DomainEvent
    {
        public Guid AggregateId { get; }

        public InventoryReorderLevelReached(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}