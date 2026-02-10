using System;

namespace E_Commerce.Domain.DomainEvents.PersonalData.Registration
{
    public sealed class RegistrationSubmitted : DomainEvent
    {
        public Guid AggregateId { get; }

        public RegistrationSubmitted(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}