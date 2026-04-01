using System;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Inventory.Inventory.StockReservation
{
    public sealed class StockReservationFailed : DomainEvent
    {
        public Guid AggregateId { get; }

        public StockReservationFailed(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}