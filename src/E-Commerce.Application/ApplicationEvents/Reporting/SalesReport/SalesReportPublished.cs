using System;

namespace E_Commerce.Application.ApplicationEvents.Reporting.SalesReport
{
    public sealed class SalesReportPublished : DomainEvent
    {
        public Guid AggregateId { get; }

        public SalesReportPublished(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}