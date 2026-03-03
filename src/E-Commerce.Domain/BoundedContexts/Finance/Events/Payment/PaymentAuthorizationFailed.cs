using System;

namespace E_Commerce.Domain.BoundedContexts.Finance.Finance.Payment
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