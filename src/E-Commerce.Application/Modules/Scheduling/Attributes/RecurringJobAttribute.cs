namespace E_Commerce.Application.Modules.Scheduling.Attributes;

/// <summary>
/// Declares that the target <see cref="ITrigger"/> should be scheduled
/// as a Hangfire recurring job with the specified ID and cron expression.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class RecurringJobAttribute : Attribute
{
    public string JobId { get; }
    public string CronExpression { get; }

    public RecurringJobAttribute(string jobId, string cronExpression)
    {
        JobId = jobId;
        CronExpression = cronExpression;
    }
}