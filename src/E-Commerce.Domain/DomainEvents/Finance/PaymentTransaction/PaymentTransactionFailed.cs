using System;

namespace E_Commerce.Domain.DomainEvents.Finance.PaymentTransaction
{
    public sealed class PaymentTransactionFailed : DomainEvent
    {
        public Guid AggregateId { get; init; }

        public PaymentTransactionFailed(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}