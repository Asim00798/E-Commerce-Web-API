#if false
using System;

namespace E_Commerce.Domain.BoundedContexts.UserManagement.PersonalData.PersonalData.Person
{
    public sealed class PersonAnonymized : DomainEvent
    {
        public Guid AggregateId { get; }

        public PersonAnonymized(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}
#endif