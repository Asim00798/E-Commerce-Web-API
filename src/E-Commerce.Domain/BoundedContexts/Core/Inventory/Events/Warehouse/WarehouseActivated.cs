#if false
using System;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Inventory.Inventory.Warehouse
{
    public sealed class WarehouseActivated : DomainEvent
    {
        public Guid AggregateId { get; }

        public WarehouseActivated(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}
#endif