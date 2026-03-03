using System;

namespace E_Commerce.Domain.BoundedContexts.Inventory.Inventory.Inventory
{
    public sealed class InventoryExpiredReported : DomainEvent
    {
        public Guid AggregateId { get; }

        public InventoryExpiredReported(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}