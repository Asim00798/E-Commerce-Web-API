using System;

namespace E_Commerce.Domain.DomainEvents.PersonalData.Registration
{
    public sealed class RegistrationExpired : DomainEvent
    {
        public Guid AggregateId { get; }

        public RegistrationExpired(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}