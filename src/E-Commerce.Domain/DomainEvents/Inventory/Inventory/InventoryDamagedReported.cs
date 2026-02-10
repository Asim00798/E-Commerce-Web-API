using System;

namespace E_Commerce.Domain.DomainEvents.Inventory.Inventory
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