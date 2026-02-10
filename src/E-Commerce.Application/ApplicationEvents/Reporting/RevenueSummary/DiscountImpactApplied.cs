using System;

namespace E_Commerce.Application.ApplicationEvents.Reporting.RevenueSummary
{
    public sealed class DiscountImpactApplied : DomainEvent
    {
        public Guid AggregateId { get; }

        public DiscountImpactApplied(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}