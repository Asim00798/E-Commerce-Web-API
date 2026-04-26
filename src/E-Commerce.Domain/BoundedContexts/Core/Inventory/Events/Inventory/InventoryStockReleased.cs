#if false
using System;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Inventory.Inventory.Inventory
{
    public sealed class InventoryStockReleased : DomainEvent
    {
        public Guid AggregateId { get; }

        public InventoryStockReleased(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}
#endif