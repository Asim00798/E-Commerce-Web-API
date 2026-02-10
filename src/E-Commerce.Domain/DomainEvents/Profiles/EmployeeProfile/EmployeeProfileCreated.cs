using System;

namespace E_Commerce.Domain.DomainEvents.Profiles.EmployeeProfile
{
    public sealed class EmployeeProfileCreated : DomainEvent
    {
        public Guid AggregateId { get; }

        public EmployeeProfileCreated(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}