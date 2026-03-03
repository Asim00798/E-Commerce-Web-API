using System;

namespace E_Commerce.Domain.BoundedContexts.Finance.Finance.Payment
{
    public sealed class PaymentCompleted : DomainEvent
    {
        public Guid AggregateId { get; init; }

        public PaymentCompleted(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}