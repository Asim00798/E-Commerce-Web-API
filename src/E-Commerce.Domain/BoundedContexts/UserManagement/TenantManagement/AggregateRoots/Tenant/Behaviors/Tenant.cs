using E_Commerce.Domain.BoundedContexts.UserManagement.TenantManagement.AggregateRoots.Tenant.Events;
using E_Commerce.Domain.BoundedContexts.UserManagement.TenantManagement.AggregateRoots.Tenant.Exceptions;
using E_Commerce.Domain.BoundedContexts.UserManagement.TenantManagement.Enums;
using E_Commerce.Domain.SharedKernel.Abstract;
using E_Commerce.Domain.SharedKernel.Events.Abstractions;
using E_Commerce.Domain.SharedKernel.Interfaces;

namespace E_Commerce.Domain.BoundedContexts.UserManagement.TenantManagement.AggregateRoots.Tenant.Behaviors
{
    public sealed class Tenant : BaseEntity, IAggregateRoot
    {
        private readonly List<IDomainEvent> _domainEvents = new();

        public string Name { get; private set; } = null!;
        public string Identifier { get; private set; } = null!;   // e.g. "amazon", "noon", "ikea"
        public string? Description { get; private set; }

        public TenantStatus Status { get; private set; }
        public DateTime CreatedOnUtc { get; private set; }
        public DateTime? ActivatedOnUtc { get; private set; }
        public DateTime? SuspendedOnUtc { get; private set; }

        public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

        private Tenant() { }

        private Tenant(string name, string identifier, string? description)
        {
            ValidateName(name);
            ValidateIdentifier(identifier);

            Name = name;
            Identifier = identifier.ToLowerInvariant();
            Description = description;

            Status = TenantStatus.Pending;
            CreatedOnUtc = DateTime.UtcNow;

            AddDomainEvent(new TenantCreatedEvent(Id, Name, Identifier));
        }

        public static Tenant Create(string name, string identifier, string? description = null)
        {
            return new Tenant(name, identifier, description);
        }

        // =========================
        // Lifecycle Behavior
        // =========================

        public void Activate()
        {
            if (Status == TenantStatus.Active)
                throw new TenantException("Tenant is already active.");

            if (Status == TenantStatus.Suspended)
                throw new TenantException("Suspended tenant must be reactivated instead.");

            Status = TenantStatus.Active;
            ActivatedOnUtc = DateTime.UtcNow;

            AddDomainEvent(new TenantActivatedEvent(Id));
        }

        public void Suspend(string? reason = null)
        {
            if (Status != TenantStatus.Active)
                throw new TenantException("Only active tenants can be suspended.");

            Status = TenantStatus.Suspended;
            SuspendedOnUtc = DateTime.UtcNow;

            AddDomainEvent(new TenantSuspendedEvent(Id, reason));
        }

        public void Reactivate()
        {
            if (Status != TenantStatus.Suspended)
                throw new TenantException("Only suspended tenants can be reactivated.");

            Status = TenantStatus.Active;
            SuspendedOnUtc = null;

            AddDomainEvent(new TenantReactivatedEvent(Id));
        }

        // =========================
        // Update Behavior
        // =========================

        public void UpdateName(string name)
        {
            ValidateName(name);
            Name = name;
        }

        public void UpdateDescription(string? description)
        {
            Description = description;
        }

        // =========================
        // Validation
        // =========================

        private static void ValidateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new TenantException("Tenant name cannot be empty.");

            if (name.Length > 200)
                throw new TenantException("Tenant name cannot exceed 200 characters.");
        }

        private static void ValidateIdentifier(string identifier)
        {
            if (string.IsNullOrWhiteSpace(identifier))
                throw new TenantException("Tenant identifier cannot be empty.");

            if (identifier.Length > 100)
                throw new TenantException("Tenant identifier cannot exceed 100 characters.");

            if (identifier.Contains(" "))
                throw new TenantException("Tenant identifier cannot contain spaces.");
        }

        private void AddDomainEvent(IDomainEvent domainEvent)
        {
            _domainEvents.Add(domainEvent);
        }

        public void ClearDomainEvents()
        {
            _domainEvents.Clear();
        }
    }
}