using System;

namespace E_Commerce.Domain.BoundedContexts.Finance.Finance.Payment
{
    public sealed class PaymentInitiated : DomainEvent
    {
        public Guid AggregateId { get; init; }

        public PaymentInitiated(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}