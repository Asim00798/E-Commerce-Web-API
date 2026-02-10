using System;

namespace E_Commerce.Application.ApplicationEvents.Reporting.SalesReport
{
    public sealed class SalesReportExported : DomainEvent
    {
        public Guid AggregateId { get; }

        public SalesReportExported(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}