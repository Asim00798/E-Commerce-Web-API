using System;

namespace E_Commerce.Domain.BoundedContexts.PersonalData.PersonalData.Registration
{
    public sealed class RegistrationExpired : DomainEvent
    {
        public Guid AggregateId { get; }
        public Guid PersonId { get; }
        public DateTime RegisteredAt { get; }

        public RegistrationExpired(Guid aggregateId, Guid personId, DateTime registeredAt)
        {
            AggregateId = aggregateId;
            PersonId = personId;
            RegisteredAt = registeredAt;
        }
    }
}