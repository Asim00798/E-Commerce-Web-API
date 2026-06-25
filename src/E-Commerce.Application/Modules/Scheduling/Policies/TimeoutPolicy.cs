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
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(_timeout);
        await next().WaitAsync(cts.Token);
    }
}