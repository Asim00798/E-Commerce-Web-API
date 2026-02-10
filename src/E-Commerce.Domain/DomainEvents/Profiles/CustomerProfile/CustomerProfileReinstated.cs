using System;

namespace E_Commerce.Domain.DomainEvents.Profiles.CustomerProfile
{
    public sealed class CustomerProfileReinstated : DomainEvent
    {
        public Guid AggregateId { get; }

        public CustomerProfileReinstated(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}