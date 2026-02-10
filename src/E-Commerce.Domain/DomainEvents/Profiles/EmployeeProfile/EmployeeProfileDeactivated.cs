using System;

namespace E_Commerce.Domain.DomainEvents.Profiles.EmployeeProfile
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