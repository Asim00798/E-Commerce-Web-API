using E_Commerce.Application.Modules.Scheduling.Abstractions;

namespace E_Commerce.Application.Modules.Scheduling.Policies;

/// <summary>
/// Contract for a resilience policy that wraps job execution.
/// Policies are composed around the core handler.
/// </summary>
public interface IJobPolicy
{
    Task ExecuteAsync<TJob>(
        TJob job,
        IJobContext context,
        Func<Task> next,
        CancellationToken cancellationToken) where TJob : IJob;
}