using System;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Inventory.Inventory.Inventory
{
    public sealed class InventoryActivated : DomainEvent
    {
        public Guid AggregateId { get; }

        public InventoryActivated(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}