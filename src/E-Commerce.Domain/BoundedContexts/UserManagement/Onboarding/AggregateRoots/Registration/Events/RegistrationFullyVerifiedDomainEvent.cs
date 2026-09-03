using E_Commerce.Domain.SharedKernel.Events;

namespace E_Commerce.Domain.BoundedContexts.UserManagement.Onboarding.AggregateRoots.Registration.Events
{
    public sealed class RegistrationFullyVerifiedDomainEvent : DomainEvent
    {
        public Guid RegistrationId { get; }
        public string Email { get; }
        public string PhoneNumber { get; }
        public string Username { get; }

        public RegistrationFullyVerifiedDomainEvent(
            Guid registrationId,
            string email,
            string phoneNumber,
            string username)
        {
            RegistrationId = registrationId;
            Email = email;
            PhoneNumber = phoneNumber;
            Username = username;
        }
    }
}