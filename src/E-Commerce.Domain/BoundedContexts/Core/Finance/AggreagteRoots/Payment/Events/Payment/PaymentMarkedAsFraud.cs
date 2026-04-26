#if false
using System;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Finance.AggreagteRoots.Payment.Events.Payment
{
    public sealed class PaymentMarkedAsFraud : DomainEvent
    {
        public Guid AggregateId { get; init; }

        public PaymentMarkedAsFraud(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}
#endif