using System;

namespace E_Commerce.Domain.BoundedContexts.Finance.Finance.Refund
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