using E_Commerce.Domain.Events;
using E_Commerce.Domain.Events.Identity.Role;
using Microsoft.AspNetCore.Identity;

namespace E_Commerce.Domain.BoundedContexts.Identity
{
    public class Role : IdentityRole<Guid>
    {
        public string? Description { get; private set; }

        // Navigation properties
        public ICollection<RolePermission>? RolePermissions { get; private set; }

        private readonly List<IDomainEvent> _domainEvents = new();
        public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

        protected void AddDomainEvent(IDomainEvent domainEvent)
        {
            _domainEvents.Add(domainEvent);
        }

        public void ClearDomainEvents()
        {
            _domainEvents.Clear();
        }

        // DDD Constructor
        public Role(string roleName, string? description = null)
        {
            Name = roleName;
            Description = description;

            AddDomainEvent(new RoleCreated(Id));
        }

        // Empty constructor for EF
        protected Role() { }

        public void Activate()
        {
            AddDomainEvent(new RoleActivated(Id));
        }

        public void Deactivate()
        {
            AddDomainEvent(new RoleDeactivated(Id));
        }

        public void Delete()
        {
            AddDomainEvent(new RoleDeleted(Id));
        }
    }
}
