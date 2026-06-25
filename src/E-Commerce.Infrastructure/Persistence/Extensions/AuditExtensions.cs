using E_Commerce.Domain.BoundedContexts.SystemOperations.Audit.Entities;
using E_Commerce.Domain.BoundedContexts.SystemOperations.Audit.Enums;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Text.Json;

namespace E_Commerce.Infrastructure.Persistence.Extensions
{
    public static class DbContextAuditExtensions
    {
        /// <summary>
        /// Applies audit logging for all entity changes (Created, Updated, Deleted).
        /// Captures the current user performing the action and property-level changes.
        /// Supports any primary key type (single or composite) as string.
        /// </summary>
        /// <param name="context">DbContext instance</param>
        /// <param name="currentUserId">Current user performing the action</param>
        public static void ApplyAuditLogging(this DbContext context, Guid? currentUserId)
        {

            var now = DateTime.UtcNow;

            var entries = context.ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added ||
                            e.State == EntityState.Modified ||
                            e.State == EntityState.Deleted)
                .ToList();

            if (!entries.Any())
                return;

            var auditLogs = new List<AuditLog>();

            foreach (var entry in entries)
            {
                if (entry.Entity is AuditLog)
                    continue; // Prevent recursion

                var entityType = entry.Entity.GetType();

                // Determine the primary key dynamically from EF metadata
                var keyProperties = context.Model
                    .FindEntityType(entry.Entity.GetType())?
                    .FindPrimaryKey()?
                    .Properties;

                string entityId = "null";

                if (keyProperties != null && keyProperties.Any())
                {
                    var keyValues = new List<string>();

                    foreach (var keyProp in keyProperties)
                    {
                        object? value = null;

                        // Try EF-tracked property first
                        var propEntry = entry.Property(keyProp.Name);
                        if (propEntry != null)
                            value = propEntry.CurrentValue;
                        else
                        {
                            // Fallback to reflection (handles inherited or shadow properties)
                            value = entry.Entity.GetType().GetProperty(keyProp.Name)?.GetValue(entry.Entity);
                        }

                        // Convert any datatype to string
                        string valueStr = value != null ? Convert.ToString(value) ?? "null" : "null";
                        keyValues.Add(valueStr);
                    }

                    // Join multiple key values for composite keys
                    entityId = string.Join(",", keyValues);
                }



                var action = entry.State switch
                {
                    EntityState.Added => AuditActionType.Created,
                    EntityState.Modified => AuditActionType.Updated,
                    EntityState.Deleted => AuditActionType.Deleted,
                    _ => AuditActionType.Unknown
                };

                string? changes = action == AuditActionType.Updated ? GetChangesAsJson(entry) : null;

                var auditLog = new AuditLog
                {
                    Id = Guid.NewGuid(),
                    EntityName = entityType.Name,
                    EntityId = entityId,
                    ActionType = action,
                    ActionPerformedByUserId = currentUserId,
                    ActionPerformedAt = now,
                    Changes = changes
                };

                auditLogs.Add(auditLog);
            }

            if (auditLogs.Any())
            {
                context.Set<AuditLog>().AddRange(auditLogs);
            }
        }

        /// <summary>
        /// Serializes property changes to JSON for audit purposes.
        /// Excludes timestamps, soft-delete, and audit-related properties.
        /// </summary>
        public static string? GetChangesAsJson(EntityEntry entry)
        {
            var excludedProps = new[]
            {
                "CreatedAt", "UpdatedAt", "DeletedAt",
                "CreatedBy", "UpdatedBy", "DeletedBy",
                "IsDeleted"
            };

            var changesDict = entry.Properties
                .Where(p => p.IsModified && !excludedProps.Contains(p.Metadata.Name))
                .ToDictionary(
                    p => p.Metadata.Name,
                    p => new { Original = p.OriginalValue, Current = p.CurrentValue }
                );

            return changesDict.Count > 0 ? JsonSerializer.Serialize(changesDict) : null;
        }
    }
}
