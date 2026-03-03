using System;

namespace E_Commerce.Domain.BoundedContexts.Finance.Finance.Refund
{
    public sealed class RefundRejected : DomainEvent
    {
        public Guid AggregateId { get; init; }

        public RefundRejected(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}