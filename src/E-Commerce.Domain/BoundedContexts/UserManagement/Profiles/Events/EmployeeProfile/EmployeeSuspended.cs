using System;

namespace E_Commerce.Domain.BoundedContexts.UserManagement.Profiles.Profiles.EmployeeProfile
{
    public sealed class EmployeeSuspended : DomainEvent
    {
        public Guid AggregateId { get; }

        public EmployeeSuspended(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}