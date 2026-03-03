using System;

namespace E_Commerce.Domain.BoundedContexts.Inventory.Inventory.Inventory
{
    public sealed class InventoryDamagedReported : DomainEvent
    {
        public Guid AggregateId { get; }

        public InventoryDamagedReported(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}