#if false
using System;

namespace E_Commerce.Domain.BoundedContexts.UserManagement.Profiles.Profiles.CustomerProfile
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
#endif