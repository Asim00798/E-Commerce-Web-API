using System;

namespace E_Commerce.Domain.DomainEvents.PersonalData.Registration
{
    public sealed class RegistrationRejected : DomainEvent
    {
        public Guid AggregateId { get; }

        public RegistrationRejected(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}