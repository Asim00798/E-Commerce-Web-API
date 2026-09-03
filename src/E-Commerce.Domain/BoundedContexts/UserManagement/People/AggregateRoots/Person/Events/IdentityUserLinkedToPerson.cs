using E_Commerce.Domain.SharedKernel.Events;

namespace E_Commerce.Domain.BoundedContexts.UserManagement.People.AggregateRoots.Person.Events
{
    public class IdentityUserLinkedToPerson : DomainEvent
    {
        public Guid PersonId { get; }
        public Guid IdentityUserId { get; }
        
        public IdentityUserLinkedToPerson(Guid personId, Guid identityUserId)
        {
            PersonId = personId;
            IdentityUserId = identityUserId;
        }
    }

}