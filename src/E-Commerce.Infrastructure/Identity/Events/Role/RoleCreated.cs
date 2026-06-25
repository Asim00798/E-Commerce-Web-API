#if false
using System;

namespace E_Commerce.Domain.Events.Identity.Role
{
    public sealed class RoleCreated : DomainEvent
    {
        public Guid AggregateId { get; }

        public RoleCreated(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}
#endif