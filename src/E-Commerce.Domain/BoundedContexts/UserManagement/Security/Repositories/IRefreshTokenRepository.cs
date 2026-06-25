using Domain.BoundedContexts.UserManagement.Security.AggregateRoots.RefreshToken.Behaviors;
using E_Commerce.Domain.SharedKernel.PersistenceAbstractions;

namespace Domain.BoundedContexts.UserManagement.Security.Repositories
{
    public interface IRefreshTokenRepository : IRepository<RefreshToken>
    {
        Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<RefreshToken>> GetActiveTokensForUserAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}
