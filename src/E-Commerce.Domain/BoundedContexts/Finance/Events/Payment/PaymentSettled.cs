using System;

namespace E_Commerce.Domain.BoundedContexts.Finance.Finance.Payment
{
    public sealed class PaymentSettled : DomainEvent
    {
        public Guid AggregateId { get; init; }

        public PaymentSettled(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}