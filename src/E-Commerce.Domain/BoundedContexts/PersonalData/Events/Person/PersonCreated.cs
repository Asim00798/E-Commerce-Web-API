using System;

namespace E_Commerce.Domain.BoundedContexts.PersonalData.PersonalData.Person
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