#if false
using System;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Finance.AggreagteRoots.Payment.Events.Payment
{
    public sealed class PaymentCancelled : DomainEvent
    {
        public Guid AggregateId { get; init; }

        public PaymentCancelled(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}
#endif