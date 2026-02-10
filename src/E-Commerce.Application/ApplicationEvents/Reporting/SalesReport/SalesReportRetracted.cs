using System;

namespace E_Commerce.Application.ApplicationEvents.Reporting.SalesReport
{
    public sealed class SalesReportRetracted : DomainEvent
    {
        public Guid AggregateId { get; }

        public SalesReportRetracted(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}