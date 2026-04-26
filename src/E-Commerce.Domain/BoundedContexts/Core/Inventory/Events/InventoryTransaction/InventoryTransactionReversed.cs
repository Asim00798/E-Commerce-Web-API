#if false
using System;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Inventory.Inventory.InventoryTransaction
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
#endif