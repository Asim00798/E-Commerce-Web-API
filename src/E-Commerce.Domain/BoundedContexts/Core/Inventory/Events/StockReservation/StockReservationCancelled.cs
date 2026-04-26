#if false
using System;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Inventory.Inventory.StockReservation
{
    public sealed class StockReservationCancelled : DomainEvent
    {
        public Guid AggregateId { get; }

        public StockReservationCancelled(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}
#endif