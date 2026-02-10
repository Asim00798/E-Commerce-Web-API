using System;

namespace E_Commerce.Domain.DomainEvents.Profiles.CustomerProfile
{
    public sealed class CustomerProfileCreated : DomainEvent
    {
        public Guid AggregateId { get; }

        public CustomerProfileCreated(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}