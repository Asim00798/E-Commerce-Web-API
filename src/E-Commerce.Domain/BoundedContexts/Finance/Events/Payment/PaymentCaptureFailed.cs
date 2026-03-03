using System;

namespace E_Commerce.Domain.BoundedContexts.Finance.Finance.Payment
{
    public sealed class PaymentCaptureFailed : DomainEvent
    {
        public Guid AggregateId { get; init; }

        public PaymentCaptureFailed(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}