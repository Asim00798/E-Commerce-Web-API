using System;

namespace E_Commerce.Application.ApplicationEvents.Reporting.RevenueSummary
{
    public sealed class RevenueCorrected : DomainEvent
    {
        public Guid AggregateId { get; }

        public RevenueCorrected(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}