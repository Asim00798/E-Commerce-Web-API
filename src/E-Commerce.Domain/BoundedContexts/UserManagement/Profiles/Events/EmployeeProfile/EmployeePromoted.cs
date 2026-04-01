using System;

namespace E_Commerce.Domain.BoundedContexts.UserManagement.Profiles.Profiles.EmployeeProfile
{
    public sealed class EmployeePromoted : DomainEvent
    {
        public Guid AggregateId { get; }

        public EmployeePromoted(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}