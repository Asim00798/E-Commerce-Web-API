using System;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Finance.AggreagteRoots.Payment.Events.Refund
{
    public sealed class RefundPartiallyCompleted : DomainEvent
    {
        public Guid AggregateId { get; init; }

        public RefundPartiallyCompleted(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}