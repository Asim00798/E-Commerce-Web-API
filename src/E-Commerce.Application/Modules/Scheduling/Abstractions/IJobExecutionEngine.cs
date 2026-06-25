namespace E_Commerce.Application.Modules.Scheduling.Abstractions;

/// <summary>
/// Execution engine contract – serves as the entry point
/// for infrastructure‑side job runners to hand a job to
/// the application's execution pipeline.
/// </summary>
public interface IJobExecutionEngine
{
    Task ExecuteAsync<TJob>(TJob job, IJobContext context, CancellationToken cancellationToken)
        where TJob : IJob;
}