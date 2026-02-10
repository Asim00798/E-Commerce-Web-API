using System;

namespace E_Commerce.Domain.DomainEvents.PersonalData.Person
{
    public sealed class PersonDeactivated : DomainEvent
    {
        public Guid AggregateId { get; }

        public PersonDeactivated(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}