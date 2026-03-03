using System;

namespace E_Commerce.Domain.BoundedContexts.Finance.Finance.Refund
{
    public sealed class RefundFailed : DomainEvent
    {
        public Guid AggregateId { get; init; }

        public RefundFailed(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}