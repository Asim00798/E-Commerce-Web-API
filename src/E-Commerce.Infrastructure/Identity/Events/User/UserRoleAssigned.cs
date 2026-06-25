#if false
using System;

namespace E_Commerce.Domain.Events.Identity.User
{
    public sealed class UserRoleAssigned : DomainEvent
    {
        public Guid AggregateId { get; }

        public UserRoleAssigned(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}
#endif