using System;

namespace E_Commerce.Domain.BoundedContexts.Finance.Finance.Refund
{
    public sealed class RefundApproved : DomainEvent
    {
        public Guid AggregateId { get; init; }

        public RefundApproved(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}