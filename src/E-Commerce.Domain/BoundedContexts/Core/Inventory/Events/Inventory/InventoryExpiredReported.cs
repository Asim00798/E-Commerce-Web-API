#if false
using System;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Inventory.Inventory.Inventory
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
#endif