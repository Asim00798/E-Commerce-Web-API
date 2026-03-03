using System;

namespace E_Commerce.Domain.BoundedContexts.PersonalData.PersonalData.Registration
{
    public sealed class RegistrationCancelled : DomainEvent
    {
        public Guid AggregateId { get; }
        public Guid PersonID { get; }
        public RegistrationCancelled(Guid aggregateId,Guid personID)
        {
            AggregateId = aggregateId;
            PersonID = personID;
        }
    }
}