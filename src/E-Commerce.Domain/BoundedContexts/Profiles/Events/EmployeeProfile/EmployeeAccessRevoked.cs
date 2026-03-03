using System;

namespace E_Commerce.Domain.BoundedContexts.Profiles.Profiles.EmployeeProfile
{
    public sealed class EmployeeAccessRevoked : DomainEvent
    {
        public Guid AggregateId { get; }

        public EmployeeAccessRevoked(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}