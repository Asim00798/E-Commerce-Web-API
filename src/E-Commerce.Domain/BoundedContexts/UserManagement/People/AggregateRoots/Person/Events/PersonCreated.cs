using E_Commerce.Domain.SharedKernel.Events;

namespace E_Commerce.Domain.BoundedContexts.UserManagement.People.AggregateRoots.Person.Events
{
    public sealed class PersonCreated : DomainEvent
    {
        public Guid PersonId { get; }

        public PersonCreated(Guid personId)
        {
            PersonId = personId;
        }
    }
}
