#if false
using System;

namespace E_Commerce.Domain.BoundedContexts.UserManagement.PersonalData.PersonalData.Person
{
    public sealed class PersonActivated : DomainEvent
    {
        public Guid AggregateId { get; }

        public PersonActivated(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}
#endif