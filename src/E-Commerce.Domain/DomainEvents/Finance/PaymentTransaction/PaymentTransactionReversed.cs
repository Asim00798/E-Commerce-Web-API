using System;

namespace E_Commerce.Domain.DomainEvents.Finance.PaymentTransaction
{
    public sealed class PaymentTransactionReversed : DomainEvent
    {
        public Guid AggregateId { get; init; }

        public PaymentTransactionReversed(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}