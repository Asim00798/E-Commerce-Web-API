#if false
using System;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Finance.AggreagteRoots.Payment.Events.PaymentTransaction
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
#endif