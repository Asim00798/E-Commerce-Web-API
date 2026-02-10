using System;

namespace E_Commerce.Domain.DomainEvents.Finance.Payment
{
    public sealed class PaymentDeclined : DomainEvent
    {
        public Guid AggregateId { get; init; }

        public PaymentDeclined(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}