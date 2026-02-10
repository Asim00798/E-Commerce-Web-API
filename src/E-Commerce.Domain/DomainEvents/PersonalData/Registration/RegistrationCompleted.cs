using System;

namespace E_Commerce.Domain.DomainEvents.PersonalData.Registration
{
    public sealed class RegistrationCompleted : DomainEvent
    {
        public Guid AggregateId { get; }

        public RegistrationCompleted(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}