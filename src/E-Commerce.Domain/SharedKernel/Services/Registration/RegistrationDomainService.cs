namespace E_Commerce.Domain.SharedKernel.Services.Registration;

public class RegistrationDomainService : IRegistrationDomainService
{
    public bool IsEligibleForRegistration(string email)
    {
        // Placeholder implementation
        return !string.IsNullOrWhiteSpace(email);
    }
}
