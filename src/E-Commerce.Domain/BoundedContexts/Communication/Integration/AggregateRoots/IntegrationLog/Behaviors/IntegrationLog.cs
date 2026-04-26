#if false
using E_Commerce.Domain.SharedKernel.Abstract;
using E_Commerce.Domain.SharedKernel.Entities;

namespace E_Commerce.Domain.BoundedContexts.Communication.Integration.AggregateRoots.IntegrationLog.Behaviors
{
    /// <summary>
    /// Tracks external system/integration calls for auditing and debugging purposes.
    /// Technical/operational entity.
    /// </summary>
    public class IntegrationLog : BaseEntity, IEntity<IntegrationLog>
    {
        /// <summary>
        /// Name of the external system or service
        /// </summary>
        public string ExternalSystem { get; private set; } = null!;

        /// <summary>
        /// Operation performed (e.g., "PaymentSync", "InventoryUpdate")
        /// </summary>
        public string Operation { get; private set; } = null!;

        /// <summary>
        /// Unique identifier for the integration request or payload
        /// </summary>
        public string? CorrelationId { get; private set; }

        /// <summary>
        /// Payload sent or received, stored as JSON
        /// </summary>
        public string? Payload { get; private set; }

        /// <summary>
        /// Response from external system (if any)
        /// </summary>
        public string? Response { get; private set; }

        /// <summary>
        /// Timestamp of when the integration call occurred
        /// </summary>
        public DateTime OccurredOnUtc { get; private set; }

        /// <summary>
        /// Whether the call was successful
        /// </summary>
        public bool Succeeded { get; private set; }

        /// <summary>
        /// Optional error message
        /// </summary>
        public string? ErrorMessage { get; private set; }

        private IntegrationLog() { } // EF Core

        public IntegrationLog(
            string externalSystem,
            string operation,
            string? correlationId = null,
            string? payload = null,
            string? response = null,
            bool succeeded = true,
            string? errorMessage = null)
        {
            ExternalSystem = externalSystem;
            Operation = operation;
            CorrelationId = correlationId;
            Payload = payload;
            Response = response;
            Succeeded = succeeded;
            ErrorMessage = errorMessage;
            OccurredOnUtc = DateTime.UtcNow;
        }

        /// <summary>
        /// Mark this integration as failed with error
        /// </summary>
        public void MarkFailed(string error)
        {
            Succeeded = false;
            ErrorMessage = error;
            OccurredOnUtc = DateTime.UtcNow;
        }

        /// <summary>
        /// Mark this integration as succeeded
        /// </summary>
        public void MarkSucceeded(string? response = null)
        {
            Succeeded = true;
            ErrorMessage = null;
            Response = response;
            OccurredOnUtc = DateTime.UtcNow;
        }
    }
}

#endif