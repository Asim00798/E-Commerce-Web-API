using System;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Finance.AggreagteRoots.Payment.Events.Refund
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