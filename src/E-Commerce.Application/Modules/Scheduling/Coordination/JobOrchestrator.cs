using E_Commerce.Application.Modules.Scheduling.Abstractions;
using E_Commerce.Application.Modules.Scheduling.Exceptions;
using E_Commerce.Application.Modules.Scheduling.Policies;
using Microsoft.Extensions.Logging;

namespace E_Commerce.Application.Modules.Scheduling.Coordination;

public class JobOrchestrator : IJobOrchestrator
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IEnumerable<IJobPolicy> _policies;
    private readonly ILogger<JobOrchestrator> _logger;

    public JobOrchestrator(
        IServiceProvider serviceProvider,
        IEnumerable<IJobPolicy> policies,
        ILogger<JobOrchestrator> logger)
    {
        _serviceProvider = serviceProvider;
        _policies = policies;
        _logger = logger;
    }

    public async Task ExecuteAsync<TJob>(TJob job, IJobContext context, CancellationToken cancellationToken)
        where TJob : IJob
    {
        var jobType = typeof(TJob).Name;

        LogJobStart(context, jobType);

        var handler = ResolveHandler<TJob>(context, jobType);
        var coreAction = CreateCoreAction(handler);
        var wrappedAction = WrapWithPolicies(job, context, coreAction, cancellationToken);

        await ExecuteAndLogResult(wrappedAction, context, jobType, cancellationToken);
    }

    #region Private Methods

    private void LogJobStart(IJobContext context, string jobType)
    {
        _logger.LogInformation(
            "Job {JobId} ({JobType}) started. Attempt {Attempt}",
            context.JobId, jobType, context.Attempt);
    }

    private IJobHandler<TJob> ResolveHandler<TJob>(IJobContext context, string jobType)
        where TJob : IJob
    {
        var handlerType = typeof(IJobHandler<TJob>);
        var handler = _serviceProvider.GetService(handlerType) as IJobHandler<TJob>;
        if (handler == null)
            throw new JobExecutionException(
                $"No handler registered for job type {jobType}", context.JobId);
        return handler;
    }

    private static Func<TJob, IJobContext, CancellationToken, Task> CreateCoreAction<TJob>(
        IJobHandler<TJob> handler) where TJob : IJob
    {
        return (job, ctx, ct) => handler.HandleAsync(job, ct);
    }

    private Func<Task> WrapWithPolicies<TJob>(
        TJob job,
        IJobContext context,
        Func<TJob, IJobContext, CancellationToken, Task> coreAction,
        CancellationToken cancellationToken) where TJob : IJob
    {
        Func<Task> wrapped = () => coreAction(job, context, cancellationToken);

        foreach (var policy in _policies.Reverse())
        {
            var next = wrapped;
            wrapped = () => policy.ExecuteAsync(job, context, next, cancellationToken);
        }

        return wrapped;
    }

    private async Task ExecuteAndLogResult(
        Func<Task> wrappedAction,
        IJobContext context,
        string jobType,
        CancellationToken cancellationToken)
    {
        try
        {
            await wrappedAction();
            _logger.LogInformation("Job {JobId} ({JobType}) finished successfully.", context.JobId, jobType);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning("Job {JobId} ({JobType}) cancelled.", context.JobId, jobType);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Job {JobId} ({JobType}) failed on attempt {Attempt}",
                context.JobId, jobType, context.Attempt);
            throw;
        }
    }

    #endregion
}