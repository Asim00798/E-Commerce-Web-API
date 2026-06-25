#if false
using System;

namespace E_Commerce.Domain.Events.Identity.Permission
{
    public sealed class PermissionCreated : DomainEvent
    {
        public Guid AggregateId { get;}

        public PermissionCreated(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}
#endif