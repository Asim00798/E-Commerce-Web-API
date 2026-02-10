using System;

namespace E_Commerce.Domain.DomainEvents.Inventory.InventoryTransaction
{
    public sealed class InventoryTransactionReversed : DomainEvent
    {
        public Guid AggregateId { get; }

        public InventoryTransactionReversed(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}