using System;

namespace E_Commerce.Domain.DomainEvents.PersonalData.Registration
{
    public sealed class RegistrationCompleted : DomainEvent
    {
        public Guid AggregateId { get; }
        public Guid PersonID { get; }
        public RegistrationCompleted(Guid aggregateId,Guid personID)
        {
            AggregateId = aggregateId;
            PersonID = personID;
        }
    }
}