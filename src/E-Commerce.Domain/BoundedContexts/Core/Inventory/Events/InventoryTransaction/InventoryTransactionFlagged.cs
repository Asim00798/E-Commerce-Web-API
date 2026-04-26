#if false
using System;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Inventory.Inventory.InventoryTransaction
{
    public sealed class InventoryTransactionFlagged : DomainEvent
    {
        public Guid AggregateId { get; }

        public InventoryTransactionFlagged(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}
#endif