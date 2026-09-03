using E_Commerce.Domain.SharedKernel.Events;

namespace E_Commerce.Domain.BoundedContexts.UserManagement.Onboarding.AggregateRoots.Registration.Events
{
    public sealed class EmailVerificationCodeGeneratedDomainEvent : DomainEvent
    {
        public Guid RegistrationId { get; }
        public string Email { get; }

        public EmailVerificationCodeGeneratedDomainEvent(
            Guid registrationId,
            string email)
        {
            RegistrationId = registrationId;
            Email = email;
        }
    }
}