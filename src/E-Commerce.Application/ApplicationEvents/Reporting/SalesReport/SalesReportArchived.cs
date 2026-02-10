using System;

namespace E_Commerce.Application.ApplicationEvents.Reporting.SalesReport
{
    public sealed class SalesReportArchived : DomainEvent
    {
        public Guid AggregateId { get; }

        public SalesReportArchived(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}