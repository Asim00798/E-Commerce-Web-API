using E_Commerce.Domain.BoundedContexts.UserManagement.Onboarding.AggregateRoots.Registration.Constants;
using E_Commerce.Domain.BoundedContexts.UserManagement.Onboarding.AggregateRoots.Registration.Events;
using E_Commerce.Domain.BoundedContexts.UserManagement.Onboarding.AggregateRoots.Registration.Exceptions;
using E_Commerce.Domain.BoundedContexts.UserManagement.Onboarding.AggregateRoots.Registration.ValueObjects;

namespace E_Commerce.Domain.BoundedContexts.UserManagement.Onboarding.AggregateRoots.Registration.Behaviors
{
    public partial class Registration
    {
        public void SetEmailVerificationCode(string hashedCode, DateTime utcNow)
        {
            AssertNotExpired(utcNow);
            AssertNotCompleted();

            if (EmailVerification.ResendCount >= RegistrationConstants.MaxResendsPerChannel)
                throw new RegistrationException("Email verification code resend limit reached.");

            if (EmailVerification.SentAt.HasValue &&
                (utcNow - EmailVerification.SentAt.Value) < RegistrationConstants.ResendCooldown)
                throw new RegistrationException("Please wait before requesting a new email code.");

            EmailVerification = EmailVerification with
            {
                CodeHash = hashedCode,
                SentAt = utcNow,
                ExpiresAt = utcNow.Add(RegistrationConstants.OTPLifetime),
                ResendCount = EmailVerification.ResendCount + 1
            };

            AddDomainEvent(new EmailVerificationCodeGeneratedDomainEvent(Id, Email.Value));
        }

        public void VerifyEmail(bool isCodeValid, DateTime utcNow)
        {
            AssertNotExpired(utcNow);

            if (EmailVerification.IsVerified)
                throw new RegistrationException("Email already verified.");

            if (!EmailVerification.IsCodeActive(utcNow))
                throw new RegistrationException("Email verification code has expired.");

            if (!isCodeValid)
            {
                var newAttemptCount = EmailVerification.AttemptCount + 1;
                EmailVerification = EmailVerification with { AttemptCount = newAttemptCount };

                if (newAttemptCount >= RegistrationConstants.MaxAttemptsPerChannel)
                    MarkExpired(utcNow);

                throw new RegistrationException("Invalid email verification code.");
            }

            // Successful verification – create a clean verified state
            EmailVerification = new VerificationChannel
            {
                VerifiedAt = utcNow
            };

            AddDomainEvent(new EmailVerifiedDomainEvent(Id, Email.Value));
            TryCompleteRegistration();
        }
    }
}