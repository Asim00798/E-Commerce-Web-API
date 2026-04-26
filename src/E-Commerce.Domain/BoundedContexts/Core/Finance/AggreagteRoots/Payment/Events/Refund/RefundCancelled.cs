#if false
using System;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Finance.AggreagteRoots.Payment.Events.Refund
{
    public sealed class RefundCancelled : DomainEvent
    {
        public Guid AggregateId { get; init; }

        public RefundCancelled(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}
#endif