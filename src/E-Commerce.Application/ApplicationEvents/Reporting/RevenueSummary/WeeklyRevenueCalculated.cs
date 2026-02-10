using System;

namespace E_Commerce.Application.ApplicationEvents.Reporting.RevenueSummary
{
    public sealed class WeeklyRevenueCalculated : DomainEvent
    {
        public Guid AggregateId { get; }

        public WeeklyRevenueCalculated(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}