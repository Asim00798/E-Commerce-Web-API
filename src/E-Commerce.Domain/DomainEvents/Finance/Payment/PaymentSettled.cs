using System;

namespace E_Commerce.Domain.DomainEvents.Finance.Payment
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