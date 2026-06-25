using E_Commerce.Domain.SharedKernel.Events;

namespace Domain.BoundedContexts.UserManagement.Security.Entities.LoginAttempt.Events
{
    public sealed class BruteForceDetected : DomainEvent
    {
        public Guid LoginAttemptId { get; }
        public Guid? UserId { get; }
        public string IpAddress { get; }
        public int FailedAttemptCount { get; }

        public BruteForceDetected(
            Guid loginAttemptId,
            Guid? userId,
            string ipAddress,
            int failedAttemptCount)
        {
            LoginAttemptId = loginAttemptId;
            UserId = userId;
            IpAddress = ipAddress;
            FailedAttemptCount = failedAttemptCount;
        }
    }
}
