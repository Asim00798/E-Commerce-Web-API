#if false
using System;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Inventory.Inventory.StockReservation
{
    public sealed class StockReserved : DomainEvent
    {
        public Guid AggregateId { get; }

        public StockReserved(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}
#endif