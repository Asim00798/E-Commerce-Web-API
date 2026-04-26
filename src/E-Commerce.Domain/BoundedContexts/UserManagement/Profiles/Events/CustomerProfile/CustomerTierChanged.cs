#if false
using System;

namespace E_Commerce.Domain.BoundedContexts.UserManagement.Profiles.Profiles.CustomerProfile
{
    public sealed class CustomerTierChanged : DomainEvent
    {
        public Guid AggregateId { get; }

        public CustomerTierChanged(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}
#endif