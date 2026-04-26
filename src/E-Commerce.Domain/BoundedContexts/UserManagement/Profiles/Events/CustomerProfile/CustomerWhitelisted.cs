#if false
using System;

namespace E_Commerce.Domain.BoundedContexts.UserManagement.Profiles.Profiles.CustomerProfile
{
    public sealed class CustomerWhitelisted : DomainEvent
    {
        public Guid AggregateId { get; }

        public CustomerWhitelisted(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}
#endif