namespace E_Commerce.Application.Modules.Scheduling.Exceptions;

/// <summary>
/// Thrown when a job fails during execution (missing handler, pipeline failure, etc.).
/// </summary>
public class JobExecutionException : Exception
{
    public string? JobId { get; }

    public JobExecutionException(string message, string? jobId = null, Exception? inner = null)
        : base(message, inner)
    {
        JobId = jobId;
    }
}