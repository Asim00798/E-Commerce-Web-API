using System;

namespace E_Commerce.Domain.BoundedContexts.Inventory.Inventory.Inventory
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