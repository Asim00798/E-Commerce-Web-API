#if false
using System;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Inventory.Inventory.Warehouse
{
    public sealed class WarehouseDeactivated : DomainEvent
    {
        public Guid AggregateId { get; }

        public WarehouseDeactivated(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}
#endif