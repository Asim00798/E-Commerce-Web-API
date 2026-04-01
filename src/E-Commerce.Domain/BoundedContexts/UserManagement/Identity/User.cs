using E_Commerce.Domain.BoundedContexts;
using E_Commerce.Domain.Interfaces;
using E_Commerce.Domain.Events;
using E_Commerce.Domain.Events.Identity.User;
using Microsoft.AspNetCore.Identity;

namespace E_Commerce.Domain.BoundedContexts.UserManagement.Identity
{
    public class User : IdentityUser<Guid>
    {
        // Required 1–1 link: User must originate from Registration
        public Guid RegistrationId { get; private set; }
        public Registration Registration { get; private set; } = null!;

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
        public User(string userName, string email, Guid registrationId)
        {
            UserName = userName;
            Email = email;
            RegistrationId = registrationId;

            AddDomainEvent(new UserRegistered(Id));
        }

        // Empty constructor for EF
        protected User() { }

        public void Activate()
        {
            // IdentityUser doesn't have IsActive by default, usually handled by LockoutEnabled or similar
            // But we can add a custom logic factor or just emit the fact
            AddDomainEvent(new UserActivated(Id));
        }

        public void Deactivate()
        {
            AddDomainEvent(new UserDeactivated(Id));
        }

        public void ConfirmEmail()
        {
            EmailConfirmed = true;
            AddDomainEvent(new UserEmailConfirmed(Id));
        }

        public void Lock()
        {
            LockoutEnabled = true;
            LockoutEnd = DateTimeOffset.MaxValue;
            AddDomainEvent(new UserLocked(Id));
        }

        public void AssignRole(Guid roleId)
        {
            AddDomainEvent(new UserRoleAssigned(Id));
        }

        public void RemoveRole(Guid roleId)
        {
            AddDomainEvent(new UserRoleRemoved(Id));
        }
    }
}
