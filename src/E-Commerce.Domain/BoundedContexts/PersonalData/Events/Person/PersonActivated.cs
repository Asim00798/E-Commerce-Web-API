using System;

namespace E_Commerce.Domain.BoundedContexts.PersonalData.PersonalData.Person
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