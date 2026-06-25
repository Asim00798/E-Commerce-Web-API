#if false
using System;

namespace E_Commerce.Domain.Events.Identity.RolePermission
{
    public sealed class RolePermissionConflictDetected : DomainEvent
    {
        public Guid AggregateId { get; }

        public RolePermissionConflictDetected(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}
#endif