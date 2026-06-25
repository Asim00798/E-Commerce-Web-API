namespace E_Commerce.Application.Modules.Scheduling.Abstractions;

/// <summary>
/// Provides runtime metadata for an executing job
/// (e.g., job ID, correlation ID, attempt number).
/// </summary>
public interface IJobContext
{
    string JobId { get; }
    string CorrelationId { get; }
    int Attempt { get; }
    DateTime QueuedAt { get; }
}