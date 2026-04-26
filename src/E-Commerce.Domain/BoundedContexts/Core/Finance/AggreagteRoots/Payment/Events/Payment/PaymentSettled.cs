#if false
using System;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Finance.AggreagteRoots.Payment.Events.Payment
{
    public sealed class PaymentSettled : DomainEvent
    {
        public Guid AggregateId { get; init; }

        public PaymentSettled(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}
#endif