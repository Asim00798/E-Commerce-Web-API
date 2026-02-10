using System;

namespace E_Commerce.Domain.DomainEvents.PersonalData.Person
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