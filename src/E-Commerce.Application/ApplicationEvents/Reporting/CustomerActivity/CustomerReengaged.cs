using System;

namespace E_Commerce.Application.ApplicationEvents.Reporting.CustomerActivity
{
    public sealed class CustomerReengaged : DomainEvent
    {
        public Guid AggregateId { get; }

        public CustomerReengaged(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}