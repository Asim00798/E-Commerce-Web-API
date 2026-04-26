#if false
using E_Commerce.Domain.BoundedContexts.SystemOperations.BackgroundProcessing.Entities;
using E_Commerce.Domain.SharedKernel.Abstract;
using E_Commerce.Domain.SharedKernel.Entities;

namespace E_Commerce.Domain.BoundedContexts.SystemOperations.BackgroundProcessing.AggregateRoots.ScheduledJob.Behaviors
{
    /// <summary>
    /// Represents a scheduled task (background job) in the system.
    /// Aggregate root: ScheduledTask
    /// </summary>
    public class ScheduledJob : BaseEntity, IAggregateRoot
    {
        public string Name { get; private set; } = null!;
        public string CronExpression { get; private set; } = null!; // schedule
        public bool IsActive { get; private set; } = true;

        /// <summary>
        /// Optional description
        /// </summary>
        public string? Description { get; private set; }

        /// <summary>
        /// Last time task ran successfully
        /// </summary>
        public DateTime? LastRunAt { get; private set; }

        /// <summary>
        /// Next scheduled run
        /// </summary>
        public DateTime? NextRunAt { get; private set; }

        /// <summary>
        /// Child entities: each execution record
        /// </summary>
        private readonly List<JobHistory> _jobHistories = new();
        public IReadOnlyList<JobHistory> JobHistories => _jobHistories.AsReadOnly();

        private ScheduledJob() { } // EF Core

        public ScheduledJob(string name, string cronExpression, string? description = null)
        {
            Name = name;
            CronExpression = cronExpression;
            Description = description;
        }

        /// <summary>
        /// Activate the task
        /// </summary>
        public void Activate() => IsActive = true;

        /// <summary>
        /// Deactivate the task
        /// </summary>
        public void Deactivate() => IsActive = false;

        /// <summary>
        /// Add a JobHistory record after execution
        /// </summary>
        public JobHistory AddJobHistory(
            bool succeeded,
            string? result = null,
            Exception? exception = null,
            DateTime? startedAt = null,
            DateTime? endedAt = null)
        {
            var history = new JobHistory(
                taskId: Id,
                succeeded: succeeded,
                result: result,
                exception: exception?.ToString(),
                startedAt: startedAt ?? DateTime.UtcNow,
                endedAt: endedAt ?? DateTime.UtcNow
            );
            _jobHistories.Add(history);

            // Update last/next run timestamps
            LastRunAt = endedAt ?? DateTime.UtcNow;
            NextRunAt = CalculateNextRun();

            return history;
        }

        /// <summary>
        /// Placeholder for cron calculation (implement a library in real project)
        /// </summary>
        private DateTime? CalculateNextRun()
        {
            // TODO: integrate a cron parser (like NCrontab) for real next-run calculation
            return DateTime.UtcNow.AddMinutes(5); // dummy example
        }
    }
}
#endif