using System;

namespace E_Commerce.Domain.BoundedContexts.Finance.Finance.Payment
{
    public sealed class PaymentDeclined : DomainEvent
    {
        public Guid AggregateId { get; init; }

        public PaymentDeclined(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}