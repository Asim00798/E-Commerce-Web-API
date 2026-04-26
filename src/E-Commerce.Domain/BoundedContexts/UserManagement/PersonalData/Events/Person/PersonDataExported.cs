#if false
using System;

namespace E_Commerce.Domain.BoundedContexts.UserManagement.PersonalData.PersonalData.Person
{
    public sealed class PersonDataExported : DomainEvent
    {
        public Guid AggregateId { get; }

        public PersonDataExported(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}
#endif