using System;

namespace E_Commerce.Domain.BoundedContexts.Inventory.Inventory.InventoryTransaction
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