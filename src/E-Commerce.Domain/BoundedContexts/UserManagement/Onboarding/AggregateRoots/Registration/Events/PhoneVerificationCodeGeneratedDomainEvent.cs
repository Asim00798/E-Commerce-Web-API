using E_Commerce.Domain.SharedKernel.Events;

namespace E_Commerce.Domain.BoundedContexts.UserManagement.Onboarding.AggregateRoots.Registration.Events
{
    public sealed class PhoneVerificationCodeGeneratedDomainEvent : DomainEvent
    {
        public Guid RegistrationId { get; }
        public string PhoneNumber { get; }

        public PhoneVerificationCodeGeneratedDomainEvent(
            Guid registrationId,
            string phoneNumber)
        {
            RegistrationId = registrationId;
            PhoneNumber = phoneNumber;
        }
    }
}