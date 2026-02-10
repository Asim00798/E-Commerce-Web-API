using System;

namespace E_Commerce.Domain.DomainEvents.Inventory.StockReservation
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