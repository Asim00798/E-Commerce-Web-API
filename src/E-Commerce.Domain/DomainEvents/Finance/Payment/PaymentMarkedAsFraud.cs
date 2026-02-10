using System;

namespace E_Commerce.Domain.DomainEvents.Finance.Payment
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