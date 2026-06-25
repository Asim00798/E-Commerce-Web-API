using Domain.BoundedContexts.UserManagement.Security.Entities.LoginAttempt.Behaviors;
using Domain.BoundedContexts.UserManagement.Security.Enums;

namespace Domain.BoundedContexts.UserManagement.Security.DomainServices
{
    public sealed class BruteForcePolicy
    {
        public int FailureThreshold { get; }
        public TimeSpan ObservationWindow { get; }

        public BruteForcePolicy(int failureThreshold = 5, TimeSpan? observationWindow = null)
        {
            if (failureThreshold < 1)
                throw new ArgumentOutOfRangeException(nameof(failureThreshold));

            FailureThreshold = failureThreshold;
            ObservationWindow = observationWindow ?? TimeSpan.FromMinutes(15);
        }

        public bool IsThresholdExceeded(IReadOnlyList<LoginAttempt> recentFailedAttempts) =>
            recentFailedAttempts.Count(attempt => attempt.Outcome == LoginAttemptOutcome.Failed)
            >= FailureThreshold;

        public int CountFailedAttempts(IReadOnlyList<LoginAttempt> attempts) =>
            attempts.Count(a => a.Outcome == LoginAttemptOutcome.Failed);
    }
}
