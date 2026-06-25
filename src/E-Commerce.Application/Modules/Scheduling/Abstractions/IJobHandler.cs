namespace E_Commerce.Application.Modules.Scheduling.Abstractions;

/// <summary>
/// Handles the execution of a specific <typeparamref name="TJob"/>.
/// Contains the actual business logic that runs in the background.
/// </summary>
/// <typeparam name="TJob">The job type this handler can process.</typeparam>
public interface IJobHandler<in TJob> where TJob : IJob
{
    Task HandleAsync(TJob job, CancellationToken cancellationToken);
}