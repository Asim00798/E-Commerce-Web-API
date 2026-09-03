using E_Commerce.Domain.BoundedContexts.UserManagement.Onboarding.AggregateRoots.Registration.Behaviors;
using E_Commerce.Domain.BoundedContexts.UserManagement.Onboarding.Repositories;
using E_Commerce.Infrastructure.Persistence.Common.Implementation;
using E_Commerce.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce.Infrastructure.Persistence.Modules.Onboarding.Repositories
{
    /// <summary>
    /// Entity Framework implementation of <see cref="IRegistrationRepository"/>.
    /// Inherits generic CRUD from <see cref="Repository{T}"/>.
    /// </summary>
    internal sealed class RegistrationRepository
        : Repository<Registration>, IRegistrationRepository
    {
        public RegistrationRepository(AppDbContext dbContext) : base(dbContext)
        {
        }

        /// <inheritdoc />
        public async Task<Registration?> GetByEmailAsync(string normalizedEmail, CancellationToken ct = default)
        {
            return await _dbContext.Set<Registration>()
                .FirstOrDefaultAsync(r => r.Email.Value == normalizedEmail, ct);
        }

        /// <inheritdoc />
        public async Task<Registration?> GetByPhoneAsync(string normalizedPhone, CancellationToken ct = default)
        {
            return await _dbContext.Set<Registration>()
                .FirstOrDefaultAsync(r => r.PhoneNumber.Value == normalizedPhone, ct);
        }

        /// <inheritdoc />
        public async Task<bool> ExistsByEmailAsync(string normalizedEmail, CancellationToken ct = default)
        {
            return await _dbContext.Set<Registration>()
                .AnyAsync(r => r.Email.Value == normalizedEmail, ct);
        }

        /// <inheritdoc />
        public async Task<bool> ExistsByPhoneAsync(string normalizedPhone, CancellationToken ct = default)
        {
            return await _dbContext.Set<Registration>()
                .AnyAsync(r => r.PhoneNumber.Value == normalizedPhone, ct);
        }

        /// <inheritdoc />
        public async Task<bool> ExistsByUsernameAsync(string normalizedUsername, CancellationToken ct = default)
        {
            return await _dbContext.Set<Registration>()
                .AnyAsync(r => r.Username.Value == normalizedUsername, ct);
        }
    }
}