using System;

namespace E_Commerce.Domain.BoundedContexts.Inventory.Inventory.StockReservation
{
    public sealed class StockReservationExpired : DomainEvent
    {
        public Guid AggregateId { get; }

        public StockReservationExpired(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}