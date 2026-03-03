using System;

namespace E_Commerce.Domain.BoundedContexts.Finance.Finance.Payment
{
    public sealed class PaymentCancelled : DomainEvent
    {
        public Guid AggregateId { get; init; }

        public PaymentCancelled(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}