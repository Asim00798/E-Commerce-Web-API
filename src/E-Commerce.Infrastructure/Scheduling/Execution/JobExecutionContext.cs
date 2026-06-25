using E_Commerce.Application.Modules.Scheduling.Abstractions;

namespace E_Commerce.Infrastructure.Scheduling.Execution;

/// <summary>
/// Concrete <see cref="IJobContext"/> built from Hangfire's <see cref="PerformContext"/>.
/// Carries runtime metadata for a job execution.
/// </summary>
public class JobExecutionContext : IJobContext
{
    public string JobId { get; }
    public string CorrelationId { get; }
    public int Attempt { get; }
    public DateTime QueuedAt { get; }

    public JobExecutionContext(string jobId, string correlationId, int attempt, DateTime queuedAt)
    {
        JobId = jobId;
        CorrelationId = correlationId;
        Attempt = attempt;
        QueuedAt = queuedAt;
    }
}