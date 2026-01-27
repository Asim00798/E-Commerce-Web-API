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

        public virtual void Validate() {
            // Default implementation (can be overridden in derived classes)
        }

    }

}
