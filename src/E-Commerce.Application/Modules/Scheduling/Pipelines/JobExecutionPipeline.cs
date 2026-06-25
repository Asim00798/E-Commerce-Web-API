using E_Commerce.Application.Modules.Scheduling.Abstractions;
using Microsoft.Extensions.Logging;

namespace E_Commerce.Application.Modules.Scheduling.Pipelines;

/// <summary>
/// Composes a sequence of <see cref="JobExecutionStep"/> instances into a single
/// pipeline that processes a job from start to finish. The core handler runs at the end.
/// </summary>
public class JobExecutionPipeline
{
    private readonly IReadOnlyList<JobExecutionStep> _steps;
    private readonly ILogger<JobExecutionPipeline> _logger;

    public JobExecutionPipeline(
        IEnumerable<JobExecutionStep> steps,
        ILogger<JobExecutionPipeline> logger)
    {
        _steps = steps.OrderBy(s => s.Order).ToList();
        _logger = logger;
    }

    public async Task<JobExecutionResult> ExecuteAsync<TJob>(
        TJob job,
        IJobContext context,
        Func<TJob, IJobContext, CancellationToken, Task> coreAction,
        CancellationToken cancellationToken) where TJob : IJob
    {
        if (cancellationToken.IsCancellationRequested)
            return JobExecutionResult.Cancelled();

        Func<object, CancellationToken, Task> chain = async (_, ct) =>
        {
            await coreAction(job, context, ct);
        };

        foreach (var step in _steps.Reverse())
        {
            var next = chain;
            chain = (ctx, ct) => step.ExecuteAsync<TJob>(ctx, next, ct);
        }

        try
        {
            _logger.LogDebug("Pipeline execution started for job {JobId}", context.JobId);
            await chain((job, context), cancellationToken);
            _logger.LogDebug("Pipeline completed successfully for job {JobId}", context.JobId);
            return JobExecutionResult.Success();
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Pipeline cancelled for job {JobId}", context.JobId);
            return JobExecutionResult.Cancelled();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Pipeline failed for job {JobId}", context.JobId);
            return JobExecutionResult.Failed(ex.Message);
        }
    }
}