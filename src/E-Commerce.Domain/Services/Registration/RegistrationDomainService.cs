namespace E_Commerce.Domain.Services.Registration;

public class RegistrationDomainService : IRegistrationDomainService
{
    public bool IsEligibleForRegistration(string email)
    {
        // Placeholder implementation
        return !string.IsNullOrWhiteSpace(email);
    }
}
