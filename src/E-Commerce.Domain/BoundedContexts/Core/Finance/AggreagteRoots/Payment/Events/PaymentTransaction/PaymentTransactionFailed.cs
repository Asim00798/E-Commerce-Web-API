using System;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Finance.AggreagteRoots.Payment.Events.PaymentTransaction
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