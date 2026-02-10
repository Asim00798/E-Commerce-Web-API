using System;

namespace E_Commerce.Domain.DomainEvents.PersonalData.Registration
{
    public sealed class RegistrationStarted : DomainEvent
    {
        public Guid AggregateId { get; }

        public RegistrationStarted(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}