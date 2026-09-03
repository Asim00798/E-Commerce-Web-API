using E_Commerce.Domain.SharedKernel.Events;

namespace E_Commerce.Domain.BoundedContexts.UserManagement.Onboarding.AggregateRoots.Registration.Events
{
    public sealed class RegistrationExpiredDomainEvent : DomainEvent
    {
        public Guid RegistrationId { get; }

        public RegistrationExpiredDomainEvent(Guid registrationId)
        {
            RegistrationId = registrationId;
        }
    }
}