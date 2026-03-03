using System;

namespace E_Commerce.Domain.BoundedContexts.Inventory.Inventory.StockReservation
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