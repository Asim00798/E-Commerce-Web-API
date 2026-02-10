using System;

namespace E_Commerce.Domain.DomainEvents.PersonalData.Person
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