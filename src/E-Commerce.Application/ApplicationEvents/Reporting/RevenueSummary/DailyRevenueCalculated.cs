using System;

namespace E_Commerce.Application.ApplicationEvents.Reporting.RevenueSummary
{
    public sealed class DailyRevenueCalculated : DomainEvent
    {
        public Guid AggregateId { get; }

        public DailyRevenueCalculated(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}