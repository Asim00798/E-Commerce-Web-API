using System;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Inventory.Inventory.Warehouse
{
    public sealed class WarehouseCreated : DomainEvent
    {
        public Guid AggregateId { get; }

        public WarehouseCreated(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}