using System;

namespace E_Commerce.Application.ApplicationEvents.Reporting.RevenueSummary
{
    public sealed class MonthlyRevenueCalculated : DomainEvent
    {
        public Guid AggregateId { get; }

        public MonthlyRevenueCalculated(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}