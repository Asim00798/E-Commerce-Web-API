using Domain.BoundedContexts.UserManagement.Security.Entities.LoginAttempt.Events;
using Domain.BoundedContexts.UserManagement.Security.Enums;

namespace Domain.BoundedContexts.UserManagement.Security.Entities.LoginAttempt.Behaviors
{
    public partial class LoginAttempt
    {
        public void GenerateSecuritySignals(int failedAttemptCount, int bruteForceThreshold)
        {
            if (Outcome == LoginAttemptOutcome.Pending)
                return;

            if (ShouldRaiseBruteForceSignal(failedAttemptCount, bruteForceThreshold))
                RaiseBruteForceSignal(failedAttemptCount);

            if (ShouldRaiseSuspiciousLoginSignal())
                RaiseSuspiciousLoginSignal();
        }

        private bool ShouldRaiseBruteForceSignal(int failedAttemptCount, int threshold) =>
            Outcome == LoginAttemptOutcome.Failed &&
            !BruteForceSignalRaised &&
            failedAttemptCount >= threshold;

        private bool ShouldRaiseSuspiciousLoginSignal() =>
            !SuspiciousSignalRaised &&
            RiskLevel is LoginRiskLevel.High or LoginRiskLevel.Critical;

        private void RaiseBruteForceSignal(int failedAttemptCount)
        {
            BruteForceSignalRaised = true;
            AddDomainEvent(new BruteForceDetected(
                Id,
                Context.UserId,
                Context.IpAddress.Value,
                failedAttemptCount));
        }

        private void RaiseSuspiciousLoginSignal()
        {
            SuspiciousSignalRaised = true;
            AddDomainEvent(new SuspiciousLoginDetected(
                Id,
                Context.UserId,
                RiskLevel,
                Context.IpAddress.Value,
                SecuritySignals));
        }
    }
}
