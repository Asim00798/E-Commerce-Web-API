using System;

namespace E_Commerce.Domain.DomainEvents.Inventory.Inventory
{
    public sealed class InventoryCreated : DomainEvent
    {
        public Guid AggregateId { get; }

        public InventoryCreated(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}