using Domain.BoundedContexts.UserManagement.Security.AggregateRoots.ApiKey.Behaviors;
using E_Commerce.Domain.SharedKernel.PersistenceAbstractions;

namespace Domain.BoundedContexts.UserManagement.Security.Repositories
{
    public interface IApiKeyRepository : IRepository<ApiKey>
    {
        Task<ApiKey?> GetActiveKeyAsync(Guid apiClientId, string keyHash, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<ApiKey>> GetActiveKeysForClientAsync(Guid apiClientId, CancellationToken cancellationToken = default);
    }
}
