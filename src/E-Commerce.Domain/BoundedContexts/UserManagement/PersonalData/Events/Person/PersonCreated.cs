#if false
using System;

namespace E_Commerce.Domain.BoundedContexts.UserManagement.PersonalData.PersonalData.Person
{
    public sealed class PersonCreated : DomainEvent
    {
        public Guid AggregateId { get; }

        public PersonCreated(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}
#endif