#if false
using System;

namespace E_Commerce.Domain.BoundedContexts.UserManagement.Profiles.Profiles.EmployeeProfile
{
    public sealed class EmployeeProfileDeactivated : DomainEvent
    {
        public Guid AggregateId { get; }

        public EmployeeProfileDeactivated(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}
#endif