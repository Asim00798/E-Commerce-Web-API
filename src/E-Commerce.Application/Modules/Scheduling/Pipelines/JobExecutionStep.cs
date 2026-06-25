namespace E_Commerce.Application.Modules.Scheduling.Pipelines;

/// <summary>
/// Base class for a single step in the job execution pipeline.
/// Steps are executed in order and can perform cross‑cutting concerns
/// (logging, validation, transaction management, metrics, etc.).
/// </summary>
public abstract class JobExecutionStep
{
    /// <summary>
    /// Human‑readable name of the step (used for logging).
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// Execution order – lower values run first.
    /// </summary>
    public abstract int Order { get; }

    /// <summary>
    /// Executes the step. The <paramref name="next"/> delegate continues to the
    /// next step or to the core handler.
    /// </summary>
    public abstract Task ExecuteAsync<TJob>(
        object context,
        Func<object, CancellationToken, Task> next,
        CancellationToken cancellationToken);
}