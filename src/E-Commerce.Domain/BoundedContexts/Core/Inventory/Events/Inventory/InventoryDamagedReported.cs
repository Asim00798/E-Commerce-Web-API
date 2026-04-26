#if false
using System;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Inventory.Inventory.Inventory
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
#endif