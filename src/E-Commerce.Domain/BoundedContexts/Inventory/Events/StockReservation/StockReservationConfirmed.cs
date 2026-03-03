using System;

namespace E_Commerce.Domain.BoundedContexts.Inventory.Inventory.StockReservation
{
    public sealed class StockReservationConfirmed : DomainEvent
    {
        public Guid AggregateId { get; }

        public StockReservationConfirmed(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}