using System.Text.Json;
using E_Commerce.Infrastructure.Persistence.Audit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace E_Commerce.Infrastructure.Persistence.Extensions;

public static class DbContextAuditExtensions
{
    /// <summary>
    /// Applies audit logging for entity changes.
    /// Original deleted entries are preserved as Deleted audit actions
    /// even when soft-delete has converted their EF state to Modified.
    /// </summary>
    public static void ApplyAuditLogging(
        this DbContext context,
        Guid? currentUserId,
        IReadOnlySet<EntityEntry>? originallyDeletedEntries = null,
        string? ipAddress = null,
        Guid? correlationId = null)
    {
        var now = DateTime.UtcNow;

        var entries = context.ChangeTracker
            .Entries()
            .Where(e =>
                e.State == EntityState.Added ||
                e.State == EntityState.Modified ||
                e.State == EntityState.Deleted)
            .ToList();

        if (entries.Count == 0)
            return;

        var auditLogs = new List<AuditLog>();

        foreach (var entry in entries)
        {
            // Prevent auditing the audit records themselves.
            if (entry.Entity is AuditLog)
                continue;

            var entityType = entry.Entity.GetType();

            var keyProperties = context.Model
                .FindEntityType(entry.Entity.GetType())?
                .FindPrimaryKey()?
                .Properties;

            var entityId = "null";

            if (keyProperties is not null && keyProperties.Any())
            {
                var keyValues = new List<string>();

                foreach (var keyProperty in keyProperties)
                {
                    object? value = null;

                    try
                    {
                        value = entry.Property(keyProperty.Name).CurrentValue;
                    }
                    catch (InvalidOperationException)
                    {
                        // Fall back to reflection for properties that
                        // cannot be accessed directly through Property().
                    }

                    if (value is null)
                    {
                        value = entry.Entity
                            .GetType()
                            .GetProperty(keyProperty.Name)?
                            .GetValue(entry.Entity);
                    }

                    var valueString =
                        value is not null
                            ? Convert.ToString(value) ?? "null"
                            : "null";

                    keyValues.Add(valueString);
                }

                entityId = string.Join(",", keyValues);
            }

            /*
             * Soft-delete changes:
             *
             *     Deleted → Modified
             *
             * Therefore, the original state must be checked first.
             */
            var wasOriginallyDeleted =
                originallyDeletedEntries?.Contains(entry) == true;

            var action = wasOriginallyDeleted
                ? AuditActionType.Deleted
                : entry.State switch
                {
                    EntityState.Added => AuditActionType.Created,
                    EntityState.Modified => AuditActionType.Updated,
                    EntityState.Deleted => AuditActionType.Deleted,
                    _ => AuditActionType.Unknown
                };

            var changes =
                action == AuditActionType.Updated
                    ? GetChangesAsJson(entry)
                    : null;

            var auditLog = new AuditLog(
                entityName: entityType.Name,
                entityId: entityId,
                actionType: action,
                actionPerformedByUserId: currentUserId,
                actionPerformedAt: now,
                changes: changes,
                ipAddress: ipAddress,
                correlationId: correlationId);

            auditLogs.Add(auditLog);
        }

        if (auditLogs.Count > 0)
        {
            context.Set<AuditLog>().AddRange(auditLogs);
        }
    }

    /// <summary>
    /// Serializes modified property values into JSON.
    /// Audit-related and lifecycle properties are excluded.
    /// </summary>
    public static string? GetChangesAsJson(EntityEntry entry)
    {
        var excludedProps = new HashSet<string>(
            StringComparer.OrdinalIgnoreCase)
        {
            "CreatedAt",
            "UpdatedAt",
            "DeletedAt",
            "CreatedBy",
            "UpdatedBy",
            "DeletedBy",
            "IsDeleted"
        };

        var changesDictionary = entry.Properties
            .Where(property =>
                property.IsModified &&
                !excludedProps.Contains(property.Metadata.Name))
            .ToDictionary(
                property => property.Metadata.Name,
                property => new
                {
                    Original = property.OriginalValue,
                    Current = property.CurrentValue
                });

        return changesDictionary.Count > 0
            ? JsonSerializer.Serialize(changesDictionary)
            : null;
    }
}