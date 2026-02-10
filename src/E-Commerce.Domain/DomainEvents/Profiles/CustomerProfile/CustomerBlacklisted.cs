using System;

namespace E_Commerce.Domain.DomainEvents.Profiles.CustomerProfile
{
    public sealed class CustomerBlacklisted : DomainEvent
    {
        public Guid AggregateId { get; }

        public CustomerBlacklisted(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}