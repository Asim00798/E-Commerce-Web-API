#if false
using System;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Inventory.Inventory.Inventory
{
    public sealed class InventoryReorderLevelReached : DomainEvent
    {
        public Guid AggregateId { get; }

        public InventoryReorderLevelReached(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}
#endif