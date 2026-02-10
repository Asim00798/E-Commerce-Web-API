using System;

namespace E_Commerce.Application.ApplicationEvents.Reporting.SalesReport
{
    public sealed class SalesReportCorrected : DomainEvent
    {
        public Guid AggregateId { get; }

        public SalesReportCorrected(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}