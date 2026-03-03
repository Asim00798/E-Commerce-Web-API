using System;

namespace E_Commerce.Domain.BoundedContexts.Finance.Finance.Payment
{
    public sealed class PaymentCaptured : DomainEvent
    {
        public Guid AggregateId { get; init; }

        public PaymentCaptured(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}