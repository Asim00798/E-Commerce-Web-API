using System;

namespace E_Commerce.Domain.DomainEvents.Profiles.EmployeeProfile
{
    public sealed class EmployeeAccessGranted : DomainEvent
    {
        public Guid AggregateId { get; }

        public EmployeeAccessGranted(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}