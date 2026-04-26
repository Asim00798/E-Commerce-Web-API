#if false
using System;

namespace E_Commerce.Domain.BoundedContexts.UserManagement.Profiles.Profiles.EmployeeProfile
{
    public sealed class EmployeeReinstated : DomainEvent
    {
        public Guid AggregateId { get; }

        public EmployeeReinstated(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}
#endif