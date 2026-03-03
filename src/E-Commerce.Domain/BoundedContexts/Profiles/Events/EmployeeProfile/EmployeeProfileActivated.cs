using System;

namespace E_Commerce.Domain.BoundedContexts.Profiles.Profiles.EmployeeProfile
{
    public sealed class EmployeeProfileActivated : DomainEvent
    {
        public Guid AggregateId { get; }

        public EmployeeProfileActivated(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}