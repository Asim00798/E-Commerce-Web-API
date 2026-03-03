using System;

namespace E_Commerce.Domain.BoundedContexts.Inventory.Inventory.StockReservation
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