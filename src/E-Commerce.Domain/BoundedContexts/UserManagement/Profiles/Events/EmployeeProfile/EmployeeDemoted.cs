using System;

namespace E_Commerce.Domain.BoundedContexts.UserManagement.Profiles.Profiles.EmployeeProfile
{
    public sealed class EmployeeDemoted : DomainEvent
    {
        public Guid AggregateId { get; }

        public EmployeeDemoted(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}