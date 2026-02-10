using System;

namespace E_Commerce.Domain.DomainEvents.Finance.Refund
{
    public sealed class RefundCompleted : DomainEvent
    {
        public Guid AggregateId { get; init; }

        public RefundCompleted(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}