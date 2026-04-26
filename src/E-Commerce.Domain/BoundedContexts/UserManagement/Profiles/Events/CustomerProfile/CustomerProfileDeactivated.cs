#if false
using System;

namespace E_Commerce.Domain.BoundedContexts.UserManagement.Profiles.Profiles.CustomerProfile
{
    public sealed class CustomerProfileDeactivated : DomainEvent
    {
        public Guid AggregateId { get; }

        public CustomerProfileDeactivated(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}
#endif