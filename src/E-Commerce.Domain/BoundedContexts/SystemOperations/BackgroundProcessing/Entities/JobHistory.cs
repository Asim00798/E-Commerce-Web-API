using E_Commerce.Domain.SharedKernel.Abstract;

namespace E_Commerce.Domain.BoundedContexts.SystemOperations.BackgroundProcessing.Entities
{
    /// <summary>
    /// Represents the execution history of a ScheduledTask
    /// </summary>
    public class JobHistory : BaseEntity
    {
        public Guid ScheduledTaskId { get; private set; }
        public bool Succeeded { get; private set; }
        public string? Result { get; private set; }
        public string? Exception { get; private set; }
        public DateTime StartedAt { get; private set; }
        public DateTime EndedAt { get; private set; }

        private JobHistory() { } // EF Core

        public JobHistory(
            Guid taskId,
            bool succeeded,
            string? result,
            string? exception,
            DateTime startedAt,
            DateTime endedAt)
        {
            ScheduledTaskId = taskId;
            Succeeded = succeeded;
            Result = result;
            Exception = exception;
            StartedAt = startedAt;
            EndedAt = endedAt;
        }
    }
}
