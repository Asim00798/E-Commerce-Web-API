using System;

namespace E_Commerce.Domain.DomainEvents.PersonalData.Registration
{
    public sealed class RegistrationCancelled : DomainEvent
    {
        public Guid AggregateId { get; }

        public RegistrationCancelled(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}