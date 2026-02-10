using System;

namespace E_Commerce.Application.ApplicationEvents.Reporting.RevenueSummary
{
    public sealed class TaxImpactApplied : DomainEvent
    {
        public Guid AggregateId { get; }

        public TaxImpactApplied(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}