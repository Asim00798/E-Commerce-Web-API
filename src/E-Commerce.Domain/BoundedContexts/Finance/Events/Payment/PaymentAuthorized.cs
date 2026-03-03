using System;

namespace E_Commerce.Domain.BoundedContexts.Finance.Finance.Payment
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