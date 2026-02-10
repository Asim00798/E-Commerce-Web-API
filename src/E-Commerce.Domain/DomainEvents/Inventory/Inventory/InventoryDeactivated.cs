using System;

namespace E_Commerce.Domain.DomainEvents.Inventory.Inventory
{
    public sealed class InventoryDeactivated : DomainEvent
    {
        public Guid AggregateId { get; }

        public InventoryDeactivated(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}