#if false
using System;

namespace E_Commerce.Domain.BoundedContexts.UserManagement.Profiles.Profiles.CustomerProfile
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
#endif