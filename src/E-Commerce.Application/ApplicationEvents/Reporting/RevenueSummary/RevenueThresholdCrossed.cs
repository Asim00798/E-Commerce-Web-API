using System;

namespace E_Commerce.Application.ApplicationEvents.Reporting.RevenueSummary
{
    public sealed class RevenueThresholdCrossed : DomainEvent
    {
        public Guid AggregateId { get; }

        public RevenueThresholdCrossed(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}