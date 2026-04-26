#if false
using System;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Finance.AggreagteRoots.Payment.Events.Payment
{
    public sealed class PaymentAuthorized : DomainEvent
    {
        public Guid AggregateId { get; init; }

        public PaymentAuthorized(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}
#endif