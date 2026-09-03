namespace E_Commerce.Application.Shared.Communication.Messaging.Abstractions
{
    public interface IIntegrationEvent
    {
        Guid EventId { get; }
        DateTime OccurredAt { get; }

        /// <summary>
        /// Correlation ID from the originating HTTP request (if available).
        /// Used to trace asynchronous work back to the user action that triggered it.
        /// </summary>
        string? CorrelationId { get; init; }
    }
}


