#if false
using System;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Finance.AggreagteRoots.Payment.Events.Payment
{
    public sealed class PaymentCaptureFailed : DomainEvent
    {
        public Guid AggregateId { get; init; }

        public PaymentCaptureFailed(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}
#endif