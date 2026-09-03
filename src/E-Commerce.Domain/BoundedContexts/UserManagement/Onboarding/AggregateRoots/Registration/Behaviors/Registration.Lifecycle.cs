using E_Commerce.Domain.BoundedContexts.UserManagement.Onboarding.AggregateRoots.Registration.Events;
using E_Commerce.Domain.BoundedContexts.UserManagement.Onboarding.AggregateRoots.Registration.Exceptions;

namespace E_Commerce.Domain.BoundedContexts.UserManagement.Onboarding.AggregateRoots.Registration.Behaviors
{
    public partial class Registration
    {
        public bool IsFullyVerified =>
            EmailVerification.IsVerified && PhoneVerification.IsVerified;

        public bool IsExpired(DateTime utcNow) =>
            ExpiresAtUtc.HasValue && utcNow > ExpiresAtUtc.Value;

        public void MarkExpired(DateTime utcNow)
        {
            if (!IsExpired(utcNow))
            {
                ExpiresAtUtc = utcNow;
                AddDomainEvent(new RegistrationExpiredDomainEvent(Id));
            }
        }

        private void AssertNotExpired(DateTime utcNow)
        {
            if (IsExpired(utcNow))
                throw new RegistrationException("Registration has expired.");
        }

        private void AssertNotCompleted()
        {
            if (RegistrationCompleted)
                throw new RegistrationException("Registration is already completed.");
        }
    }
}