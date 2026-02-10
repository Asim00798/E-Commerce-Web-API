using System;

namespace E_Commerce.Domain.DomainEvents.PersonalData.Registration
{
    public sealed class RegistrationVerified : DomainEvent
    {
        public Guid AggregateId { get; }

        public RegistrationVerified(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}