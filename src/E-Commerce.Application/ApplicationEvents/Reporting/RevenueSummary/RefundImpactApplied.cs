using System;

namespace E_Commerce.Application.ApplicationEvents.Reporting.RevenueSummary
{
    public sealed class RefundImpactApplied : DomainEvent
    {
        public Guid AggregateId { get; }

        public RefundImpactApplied(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}