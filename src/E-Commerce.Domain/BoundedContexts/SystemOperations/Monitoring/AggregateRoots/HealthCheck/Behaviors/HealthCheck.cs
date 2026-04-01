using E_Commerce.Domain.SharedKernel.Abstract;
using E_Commerce.Domain.SharedKernel.Interfaces;

namespace E_Commerce.Domain.BoundedContexts.SystemOperations.Monitoring.AggregateRoots.HealthCheck.Behaviors
{
    /// <summary>
    /// Tracks system health checks for monitoring, alerting, and operational insights.
    /// Technical/operational entity.
    /// </summary>
    public class HealthCheck : BaseEntity, IEntity<HealthCheck>
    {
        /// <summary>
        /// Name or type of the health check (e.g., "Database", "API", "Cache")
        /// </summary>
        public string CheckName { get; private set; } = null!;

        /// <summary>
        /// Status of the check
        /// </summary>
        public HealthStatus Status { get; private set; }

        /// <summary>
        /// Optional detailed message or error
        /// </summary>
        public string? Message { get; private set; }

        /// <summary>
        /// Time when this health check was performed
        /// </summary>
        public DateTime CheckedAtUtc { get; private set; }

        private HealthCheck() { } // EF Core

        public HealthCheck(string checkName, HealthStatus status, string? message = null)
        {
            CheckName = checkName;
            Status = status;
            Message = message;
            CheckedAtUtc = DateTime.UtcNow;
        }

        /// <summary>
        /// Update the health check result
        /// </summary>
        public void UpdateStatus(HealthStatus status, string? message = null)
        {
            Status = status;
            Message = message;
            CheckedAtUtc = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Health check statuses
    /// </summary>
    public enum HealthStatus
    {
        Healthy,
        Warning,
        Unhealthy
    }
}
