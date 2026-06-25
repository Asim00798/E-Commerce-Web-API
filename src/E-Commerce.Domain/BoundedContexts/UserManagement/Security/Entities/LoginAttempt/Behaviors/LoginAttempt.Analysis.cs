using Domain.BoundedContexts.UserManagement.Security.Entities.LoginAttempt.ValueObjects;
using Domain.BoundedContexts.UserManagement.Security.Enums;
using E_Commerce.Domain.SharedKernel.Exceptions;

namespace Domain.BoundedContexts.UserManagement.Security.Entities.LoginAttempt.Behaviors
{
    public partial class LoginAttempt
    {
        public static LoginAttempt Record(LoginContext context)
        {
            ArgumentNullException.ThrowIfNull(context);
            return new LoginAttempt(context);
        }

        public void MarkSuccessful()
        {
            EnsurePending();

            Outcome = LoginAttemptOutcome.Succeeded;
            FailureReason = null;
        }

        public void MarkFailed(string reason)
        {
            EnsurePending();

            if (string.IsNullOrWhiteSpace(reason))
                throw new BusinessRuleViolationException("Failure reason is required.");

            Outcome = LoginAttemptOutcome.Failed;
            FailureReason = reason.Trim();
        }

        private void EnsurePending()
        {
            if (Outcome != LoginAttemptOutcome.Pending)
                throw new BusinessRuleViolationException(
                    $"Login attempt outcome is already determined: '{Outcome}'.");
        }
    }
}
