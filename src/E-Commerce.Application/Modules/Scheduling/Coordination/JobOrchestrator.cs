using E_Commerce.Application.Modules.Scheduling.Abstractions;
using E_Commerce.Application.Modules.Scheduling.Exceptions;
using E_Commerce.Application.Modules.Scheduling.Pipelines;
using E_Commerce.Application.Modules.Scheduling.Policies;
using Microsoft.Extensions.Logging;

namespace E_Commerce.Application.Modules.Scheduling.Coordination;

/// <summary>
/// Central execution gateway that resolves the handler, applies policies,
/// runs the pipeline, and provides structured lifecycle logging.
/// </summary>
public class JobOrchestrator : IJobExecutionEngine
{
    private readonly IServiceProvider _serviceProvider;
    private readonly JobExecutionPipeline _pipeline;
    private readonly IEnumerable<IJobPolicy> _policies;
    private readonly ILogger<JobOrchestrator> _logger;

    public JobOrchestrator(
        IServiceProvider serviceProvider,
        JobExecutionPipeline pipeline,
        IEnumerable<IJobPolicy> policies,
        ILogger<JobOrchestrator> logger)
    {
        _serviceProvider = serviceProvider;
        _pipeline = pipeline;
        _policies = policies;
        _logger = logger;
    }

    public async Task ExecuteAsync<TJob>(TJob job, IJobContext context, CancellationToken cancellationToken)
        where TJob : IJob
    {
        var jobType = typeof(TJob).Name;

        // 1. Log start (with attempt number)
        _logger.LogInformation(
            "Job {JobId} ({JobType}) started. Attempt {Attempt}",
            context.JobId, jobType, context.Attempt);

        // 2. Resolve handler
        var handlerType = typeof(IJobHandler<TJob>);
        var handler = _serviceProvider.GetService(handlerType) as IJobHandler<TJob>;
        if (handler == null)
            throw new JobExecutionException(
                $"No handler registered for job type {jobType}", context.JobId);

        // 3. Core action
        Func<TJob, IJobContext, CancellationToken, Task> coreAction = (j, ctx, ct) => handler.HandleAsync(j, ct);

        // 4. Wrap core action with policies (timeout, circuit breaker)
        Func<Task> wrappedAction = async () =>
        {
            var result = await _pipeline.ExecuteAsync(job, context, coreAction, cancellationToken);
            if (!result.IsSuccess && !result.IsCancelled)
                throw new JobExecutionException(result.ErrorMessage ?? "Pipeline failed", context.JobId);
        };

        // Compose policies: reverse order so first registered policy is outermost
        foreach (var policy in _policies.Reverse())
        {
            var next = wrappedAction;
            wrappedAction = () => policy.ExecuteAsync(job, context, next, cancellationToken);
        }

        try
        {
            await wrappedAction();
            _logger.LogInformation("Job {JobId} ({JobType}) finished successfully.", context.JobId, jobType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Job {JobId} ({JobType}) failed on attempt {Attempt}",
                context.JobId, jobType, context.Attempt);
            throw; // Let Hangfire retry
        }
    }
}