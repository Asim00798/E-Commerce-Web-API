namespace E_Commerce.Infrastructure.Persistence.Audit;

public class AuditLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string EntityName { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public AuditActionType ActionType { get; set; } = AuditActionType.Created;
    public Guid? ActionPerformedByUserId { get; set; }
    public DateTime ActionPerformedAt { get; set; } = DateTime.UtcNow;
    public string? Changes { get; set; } /// Optional serialized snapshot of property changes.
    public string? IpAddress { get; set; }
    public Guid? CorrelationId { get; set; }

    /// <summary>
    /// Public constructor for EF Core.
    /// </summary>
    public AuditLog()
    {
    }

    /// <summary>
    /// Public constructor for creating a new audit log entry.
    /// </summary>
    public AuditLog(
        string entityName,
        string entityId,
        AuditActionType actionType,
        Guid? actionPerformedByUserId,
        DateTime actionPerformedAt,
        string? changes = null,
        string? ipAddress = null,
        Guid? correlationId = null)
    {
        EntityName = entityName;
        EntityId = entityId;
        ActionType = actionType;
        ActionPerformedByUserId = actionPerformedByUserId;
        ActionPerformedAt = actionPerformedAt;
        Changes = changes;
        IpAddress = ipAddress;
        CorrelationId = correlationId;
    }
}

public enum AuditActionType
{
    Unknown = 0,
    Created = 1,
    Updated = 2,
    Deleted = 3,
    Viewed = 4
}