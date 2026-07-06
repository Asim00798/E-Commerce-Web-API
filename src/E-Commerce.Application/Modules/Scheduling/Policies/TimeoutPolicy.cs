using E_Commerce.Application.Modules.Scheduling.Abstractions;

namespace E_Commerce.Application.Modules.Scheduling.Policies;

/// <summary>
/// Aborts a job that exceeds a specified time limit.
/// </summary>
public class TimeoutPolicy : IJobPolicy
{
    private readonly TimeSpan _timeout;

    public TimeoutPolicy(TimeSpan timeout) => _timeout = timeout;

    public async Task ExecuteAsync<TJob>(
        TJob job,
        IJobContext context,
        Func<Task> next,
        CancellationToken cancellationToken) where TJob : IJob
    {
        /// <summary>
        /// Creates a linked <see cref="CancellationTokenSource"/> that cancels after the configured timeout.
        /// </summary>
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(_timeout);
        /// <summary>
        /// Executes the next delegate and throws <see cref="OperationCanceledException"/> if the timeout fires.
        /// </summary>
        await next().WaitAsync(cts.Token);
    }
}