using System;

namespace E_Commerce.Domain.DomainEvents.Finance.Refund
{
    public sealed class RefundRejected : DomainEvent
    {
        public Guid AggregateId { get; init; }

        public RefundRejected(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}