using System;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Finance.AggreagteRoots.Payment.Events.Payment
{
    public sealed class PaymentAuthorizationFailed : DomainEvent
    {
        public Guid AggregateId { get; init; }

        public PaymentAuthorizationFailed(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}