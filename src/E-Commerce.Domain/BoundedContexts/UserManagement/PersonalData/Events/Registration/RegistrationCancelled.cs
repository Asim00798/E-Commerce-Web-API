#if false
using System;

namespace E_Commerce.Domain.BoundedContexts.UserManagement.PersonalData.PersonalData.Registration
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
#endif