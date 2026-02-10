using System;

namespace E_Commerce.Domain.DomainEvents.Finance.Refund
{
    public sealed class RefundRequested : DomainEvent
    {
        public Guid AggregateId { get; init; }

        public RefundRequested(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}