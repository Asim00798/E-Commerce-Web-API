using E_Commerce.Domain.BoundedContexts.Administration.Exceptions;
using E_Commerce.Domain.BoundedContexts.UserManagement.Identity;
using E_Commerce.Domain.SharedKernel.Abstract;
using E_Commerce.Domain.SharedKernel.Enums;
using E_Commerce.Domain.SharedKernel.Interfaces;

namespace E_Commerce.Domain.BoundedContexts.SystemOperations.Audit.AggregateRoots.AuditLog.Behaviors
{
    public class AuditLog : BaseEntity, IEntity<AuditLog>
    {
        /// <summary>
        /// Name of the entity being audited (e.g., "Order", "Person").
        /// </summary>
        public string EntityName { get; set; } = string.Empty;

        /// <summary>
        /// Identifier of the audited entity (stored as string for flexibility).
        /// </summary>
        public string EntityId { get; set; } = string.Empty;

        /// <summary>
        /// Type of action (Create, Update, Delete, etc.).
        /// </summary>
        public AuditActionType ActionType { get; set; } = AuditActionType.Created;

        /// <summary>
        /// User who performed the action (business action performer).
        /// </summary>
        public Guid? ActionPerformedByUserId { get; set; }

        /// <summary>
        /// When the action occurred (not when the log was written).
        /// </summary>
        public DateTime ActionPerformedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Optional serialized snapshot of changes.
        /// </summary>
        public string? Changes { get; set; }

        /// <summary>
        /// IP address from which the action was performed.
        /// </summary>
        public string? IpAddress { get; set; }

        // Navigation property (who did the action)
        public User? ActionPerformedBy { get; set; }

        /// <summary>
        /// Validation to ensure audit log integrity.
        /// </summary>
        public override void Validate()
        {
            if (string.IsNullOrWhiteSpace(EntityName))
                throw new AuditLogException("Audit logging","EntityName is required.");
        }
    }

}
