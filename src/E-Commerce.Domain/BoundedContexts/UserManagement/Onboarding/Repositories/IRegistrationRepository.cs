using E_Commerce.Domain.SharedKernel.PersistenceAbstractions;
using OnboardingRegistration = E_Commerce.Domain.BoundedContexts.UserManagement.Onboarding.AggregateRoots.Registration.Behaviors.Registration;

namespace E_Commerce.Domain.BoundedContexts.UserManagement.Onboarding.Repositories
{
    public interface IRegistrationRepository : IRepository<OnboardingRegistration>
    {
        Task<OnboardingRegistration?> GetByEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default);
        Task<OnboardingRegistration?> GetByPhoneAsync(string normalizedPhone, CancellationToken cancellationToken = default);
        Task<bool> ExistsByEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default);
        Task<bool> ExistsByPhoneAsync(string normalizedPhone, CancellationToken cancellationToken = default);
        Task<bool> ExistsByUsernameAsync(string normalizedUsername, CancellationToken cancellationToken = default);
    }
}