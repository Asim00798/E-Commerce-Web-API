#if false
using System;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Finance.AggreagteRoots.Payment.Events.Refund
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
#endif