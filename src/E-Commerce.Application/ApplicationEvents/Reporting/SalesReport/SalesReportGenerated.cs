using System;

namespace E_Commerce.Application.ApplicationEvents.Reporting.SalesReport
{
    public sealed class SalesReportGenerated : DomainEvent
    {
        public Guid AggregateId { get; }

        public SalesReportGenerated(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}