namespace E_Commerce.Application.Modules.Scheduling.Pipelines;

/// <summary>
/// Represents the outcome of running a job through the execution pipeline.
/// </summary>
public class JobExecutionResult
{
    public bool IsSuccess { get; private set; }
    public bool IsCancelled { get; private set; }
    public string? ErrorMessage { get; private set; }

    private JobExecutionResult() { }

    public static JobExecutionResult Success() => new() { IsSuccess = true };
    public static JobExecutionResult Cancelled() => new() { IsCancelled = true };
    public static JobExecutionResult Failed(string error) => new() { ErrorMessage = error };
}