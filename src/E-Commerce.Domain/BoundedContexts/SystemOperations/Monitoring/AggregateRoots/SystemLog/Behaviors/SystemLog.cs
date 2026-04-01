using E_Commerce.Domain.SharedKernel.Abstract;
using E_Commerce.Domain.SharedKernel.Interfaces;

namespace E_Commerce.Domain.BoundedContexts.SystemOperations.Monitoring.AggregateRoots.SystemLog.Behaviors
{
    /// <summary>
    /// SystemLog records critical system-level events and failures.
    /// This is NOT a domain event but a technical log entity.
    /// </summary>
    public class SystemLog : BaseEntity, IEntity<SystemLog>
    {
        /// <summary>
        /// Log level: Info, Warning, Error, Critical, Debug
        /// </summary>
        public string Level { get; private set; } = null!;

        /// <summary>
        /// Human-readable log message
        /// </summary>
        public string Message { get; private set; } = null!;

        /// <summary>
        /// Exception details (stack trace, inner exceptions)
        /// </summary>
        public string? Exception { get; private set; }

        /// <summary>
        /// Source of the log (service, API, background job, etc.)
        /// </summary>
        public string? Source { get; private set; }

        /// <summary>
        /// Correlation Id to link related operations across services
        /// </summary>
        public Guid? CorrelationId { get; private set; }

        /// <summary>
        /// User or system identity that caused the action (optional)
        /// </summary>
        public string? UserId { get; private set; }

        /// <summary>
        /// Entity type involved in the error (optional)
        /// </summary>
        public string? EntityType { get; private set; }

        /// <summary>
        /// Specific entity Id if applicable (optional)
        /// </summary>
        public Guid? EntityId { get; private set; }

        /// <summary>
        /// UTC timestamp when the log occurred
        /// </summary>
        public DateTime OccurredOnUtc { get; private set; }

        /// <summary>
        /// Optional additional metadata in JSON (flexible for future needs)
        /// </summary>
        public string? MetadataJson { get; private set; }

        private SystemLog() { } // EF Core

        /// <summary>
        /// Factory method for creating SystemLog
        /// </summary>
        public static SystemLog Create(
            string level,
            string message,
            string? exception = null,
            string? source = null,
            Guid? correlationId = null,
            string? userId = null,
            string? entityType = null,
            Guid? entityId = null,
            object? metadata = null)
        {
            return new SystemLog
            {
                Level = level,
                Message = message,
                Exception = exception,
                Source = source,
                CorrelationId = correlationId,
                UserId = userId,
                EntityType = entityType,
                EntityId = entityId,
                MetadataJson = metadata is null ? null : System.Text.Json.JsonSerializer.Serialize(metadata),
                OccurredOnUtc = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Optional helper to mark log as reviewed or processed
        /// </summary>
        public bool Processed { get; private set; } = false;
        public void MarkProcessed() => Processed = true;
    }
}