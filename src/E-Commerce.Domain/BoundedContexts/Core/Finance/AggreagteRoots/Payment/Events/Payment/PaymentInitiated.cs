using System;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Finance.AggreagteRoots.Payment.Events.Payment
{
    public sealed class PaymentInitiated : DomainEvent
    {
        public Guid AggregateId { get; init; }

        public PaymentInitiated(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}