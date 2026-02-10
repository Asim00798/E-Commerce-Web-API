using System;

namespace E_Commerce.Domain.DomainEvents.Finance.Refund
{
    public sealed class RefundCancelled : DomainEvent
    {
        public Guid AggregateId { get; init; }

        public RefundCancelled(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}