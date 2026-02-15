using E_Commerce.Domain.DomainEvents.Abstractions;
using E_Commerce.Domain.Interfaces;

namespace E_Commerce.Domain.Entities.Abstract
{
    public abstract class BaseEntity : IValidatableEntity
    {
        // Primary Key
        public Guid Id { get; set; } = Guid.NewGuid();

        // Basic auditing
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public Guid? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? UpdatedBy { get; set; }

        // Soft Delete Info
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public Guid? DeletedBy { get; set; }

        /*
         Protected parameterless constructor required by EF Core for materialization,
         preventing direct instantiation while centralizing the repeated empty constructors
         across all derived entities.
        */
        protected BaseEntity() { } // For EF Core

        public virtual void Validate() {
            // Default implementation (can be overridden in derived classes)
        }

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

    }

}
