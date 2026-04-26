#if false
using System;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Finance.AggreagteRoots.Payment.Events.Refund
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
#endif