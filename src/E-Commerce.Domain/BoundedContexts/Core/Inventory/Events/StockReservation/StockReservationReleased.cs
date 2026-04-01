using System;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Inventory.Inventory.StockReservation
{
    public sealed class StockReservationReleased : DomainEvent
    {
        public Guid AggregateId { get; }

        public StockReservationReleased(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}