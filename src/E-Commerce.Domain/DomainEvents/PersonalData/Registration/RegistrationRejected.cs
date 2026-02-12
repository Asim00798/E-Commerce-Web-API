using System;

namespace E_Commerce.Domain.DomainEvents.PersonalData.Registration
{
    public sealed class RegistrationRejected : DomainEvent
    {
        public Guid AggregateId { get; }
        public Guid PersonId { get; }
        public string Reason { get; }
        public RegistrationRejected(Guid aggregateId, Guid personId, string reason)
        {
            AggregateId = aggregateId;
            PersonId = personId;
            Reason = reason;
        }
    }
}