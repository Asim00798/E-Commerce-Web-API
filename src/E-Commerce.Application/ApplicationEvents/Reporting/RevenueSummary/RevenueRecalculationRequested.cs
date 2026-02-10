using System;

namespace E_Commerce.Application.ApplicationEvents.Reporting.RevenueSummary
{
    public sealed class RevenueRecalculationRequested : DomainEvent
    {
        public Guid AggregateId { get; }

        public RevenueRecalculationRequested(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}