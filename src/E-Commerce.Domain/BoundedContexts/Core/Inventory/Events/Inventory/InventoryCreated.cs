#if false
using System;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Inventory.Inventory.Inventory
{
    public sealed class InventoryCreated : DomainEvent
    {
        public Guid AggregateId { get; }

        public InventoryCreated(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}
#endif