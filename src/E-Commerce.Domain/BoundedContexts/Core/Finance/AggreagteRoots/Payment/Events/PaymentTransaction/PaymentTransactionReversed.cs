using System;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Finance.AggreagteRoots.Payment.Events.PaymentTransaction
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