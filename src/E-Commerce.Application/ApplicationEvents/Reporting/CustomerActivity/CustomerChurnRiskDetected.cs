using System;

namespace E_Commerce.Application.ApplicationEvents.Reporting.CustomerActivity
{
    public sealed class CustomerChurnRiskDetected : DomainEvent
    {
        public Guid AggregateId { get; }

        public CustomerChurnRiskDetected(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}