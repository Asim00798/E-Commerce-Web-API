using System;

namespace E_Commerce.Domain.DomainEvents.Profiles.EmployeeProfile
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