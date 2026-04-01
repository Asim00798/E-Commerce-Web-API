using System;

namespace E_Commerce.Domain.BoundedContexts.UserManagement.Profiles.Profiles.CustomerProfile
{
    public sealed class CustomerProfileSuspended : DomainEvent
    {
        public Guid AggregateId { get; }

        public CustomerProfileSuspended(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}