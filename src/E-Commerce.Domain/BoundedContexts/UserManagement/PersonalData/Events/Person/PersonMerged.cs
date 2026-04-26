#if false
using System;

namespace E_Commerce.Domain.BoundedContexts.UserManagement.PersonalData.PersonalData.Person
{
    public sealed class PersonMerged : DomainEvent
    {
        public Guid AggregateId { get; }

        public PersonMerged(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}
#endif