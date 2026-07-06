using E_Commerce.Application.Modules.Scheduling.Abstractions;

namespace E_Commerce.Application.Modules.Scheduling.Policies;

/// <summary>
/// Contract for a resilience policy that wraps job execution.
/// Policies are composed around the core handler by the <see cref="JobOrchestrator"/>.
/// </summary>
public interface IJobPolicy
{
    /// <summary>
    /// Executes the policy, which may perform work before and/or after calling the
    /// next delegate in the pipeline.
    /// </summary>
    /// <typeparam name="TJob">The type of job being processed.</typeparam>
    /// <param name="job">The job instance currently being executed.</param>
    /// <param name="context">
    /// Runtime metadata for the job execution, including the unique job identifier,
    /// correlation identifier, attempt number, and enqueue timestamp.
    /// </param>
    /// <param name="next">
    /// A delegate that represents the next policy in the chain, or the core job
    /// handler if this policy is the innermost wrapper. The policy must call this
    /// delegate to continue execution; otherwise the job will be abandoned.
    /// </param>
    /// <param name="cancellationToken">
    /// A cancellation token that signals the entire job execution should be aborted.
    /// </param>
    Task ExecuteAsync<TJob>(
        TJob job,
        IJobContext context,
        Func<Task> next,
        CancellationToken cancellationToken) where TJob : IJob;
}