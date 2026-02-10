using System;

namespace E_Commerce.Domain.DomainEvents.Finance.Payment
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