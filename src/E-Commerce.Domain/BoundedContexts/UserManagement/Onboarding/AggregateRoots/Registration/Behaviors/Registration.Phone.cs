using E_Commerce.Domain.BoundedContexts.UserManagement.Onboarding.AggregateRoots.Registration.Constants;
using E_Commerce.Domain.BoundedContexts.UserManagement.Onboarding.AggregateRoots.Registration.Events;
using E_Commerce.Domain.BoundedContexts.UserManagement.Onboarding.AggregateRoots.Registration.Exceptions;
using E_Commerce.Domain.BoundedContexts.UserManagement.Onboarding.AggregateRoots.Registration.ValueObjects;

namespace E_Commerce.Domain.BoundedContexts.UserManagement.Onboarding.AggregateRoots.Registration.Behaviors
{
    public partial class Registration
    {
        public void SetPhoneVerificationCode(string hashedCode, DateTime utcNow)
        {
            AssertNotExpired(utcNow);
            AssertNotCompleted();

            if (PhoneVerification.ResendCount >= RegistrationConstants.MaxResendsPerChannel)
                throw new RegistrationException("Phone verification code resend limit reached.");

            if (PhoneVerification.SentAt.HasValue &&
                (utcNow - PhoneVerification.SentAt.Value) < RegistrationConstants.ResendCooldown)
                throw new RegistrationException("Please wait before requesting a new phone code.");

            PhoneVerification = PhoneVerification with
            {
                CodeHash = hashedCode,
                SentAt = utcNow,
                ExpiresAt = utcNow.Add(RegistrationConstants.OTPLifetime),
                ResendCount = PhoneVerification.ResendCount + 1
            };

            AddDomainEvent(new PhoneVerificationCodeGeneratedDomainEvent(Id, PhoneNumber.Value));
        }

        public void VerifyPhone(bool isCodeValid, DateTime utcNow)
        {
            AssertNotExpired(utcNow);

            if (PhoneVerification.IsVerified)
                throw new RegistrationException("Phone already verified.");

            if (!PhoneVerification.IsCodeActive(utcNow))
                throw new RegistrationException("Phone verification code has expired.");

            if (!isCodeValid)
            {
                var newAttemptCount = PhoneVerification.AttemptCount + 1;
                PhoneVerification = PhoneVerification with { AttemptCount = newAttemptCount };

                if (newAttemptCount >= RegistrationConstants.MaxAttemptsPerChannel)
                    MarkExpired(utcNow);

                throw new RegistrationException("Invalid phone verification code.");
            }

            PhoneVerification = new VerificationChannel
            {
                VerifiedAt = utcNow
            };

            AddDomainEvent(new PhoneVerifiedDomainEvent(Id, PhoneNumber.Value));
            TryCompleteRegistration();
        }

        private void TryCompleteRegistration()
        {
            if (IsFullyVerified && !RegistrationCompleted)
            {
                RegistrationCompleted = true;
                AddDomainEvent(new RegistrationFullyVerifiedDomainEvent(
                    Id, Email.Value, PhoneNumber.Value, Username.Value));
            }
        }
    }
}