using System;

namespace E_Commerce.Application.ApplicationEvents.Reporting.SalesReport
{
    public sealed class SalesReportRegenerated : DomainEvent
    {
        public Guid AggregateId { get; }

        public SalesReportRegenerated(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}