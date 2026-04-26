#if false
using System;

namespace E_Commerce.Domain.BoundedContexts.UserManagement.PersonalData.PersonalData.Registration
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
#endif