using E_Commerce.Domain.SharedKernel.Exceptions;

namespace E_Commerce.Domain.BoundedContexts.UserManagement.Onboarding.AggregateRoots.Registration.Exceptions
{
    public class RegistrationException : DomainException
    {
        public RegistrationException(string message) : base(message) { }
    }
}