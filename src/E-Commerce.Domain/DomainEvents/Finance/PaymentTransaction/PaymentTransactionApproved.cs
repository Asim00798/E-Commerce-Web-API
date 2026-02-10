using System;

namespace E_Commerce.Domain.DomainEvents.Finance.PaymentTransaction
{
    public sealed class PaymentTransactionApproved : DomainEvent
    {
        public Guid AggregateId { get; init; }

        public PaymentTransactionApproved(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}