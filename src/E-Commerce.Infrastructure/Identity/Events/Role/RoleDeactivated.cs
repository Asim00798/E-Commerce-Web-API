#if false
using System;

namespace E_Commerce.Domain.Events.Identity.Role
{
    public sealed class RoleDeactivated : DomainEvent
    {
        public Guid AggregateId { get; }

        public RoleDeactivated(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}
#endif