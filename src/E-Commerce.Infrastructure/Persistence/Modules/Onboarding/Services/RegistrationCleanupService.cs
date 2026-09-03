using OnboardingRegistration = E_Commerce.Domain.BoundedContexts.UserManagement.Onboarding.AggregateRoots.Registration.Behaviors.Registration;
using E_Commerce.Application.BoundedContexts.Onboarding.Abstractions;
using E_Commerce.Infrastructure.Persistence.Context;

namespace E_Commerce.Infrastructure.Persistence.Modules.Onboarding.Services
{
    /// <summary>
    /// EF Core implementation of <see cref="IRegistrationCleanupService"/>.
    /// </summary>
    internal sealed class RegistrationCleanupService : IRegistrationCleanupService
    {
        private readonly AppDbContext _dbContext;

        public RegistrationCleanupService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <inheritdoc />
        public async Task<int> DeleteExpiredAsync(DateTime utcNow, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Set<OnboardingRegistration>()
                .Where(r => r.ExpiresAtUtc.HasValue && r.ExpiresAtUtc.Value < utcNow)
                .ExecuteDeleteAsync(cancellationToken);
        }
    }
}
