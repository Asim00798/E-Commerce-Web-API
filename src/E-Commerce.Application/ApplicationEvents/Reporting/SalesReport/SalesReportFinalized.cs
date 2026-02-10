using System;

namespace E_Commerce.Application.ApplicationEvents.Reporting.SalesReport
{
    public sealed class SalesReportFinalized : DomainEvent
    {
        public Guid AggregateId { get; }

        public SalesReportFinalized(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}