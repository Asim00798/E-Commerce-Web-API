#if false
using System;

namespace E_Commerce.Domain.Events.Identity.User
{
    public sealed class UserRoleRemoved : DomainEvent
    {
        public Guid AggregateId { get; }

        public UserRoleRemoved(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}
#endif