using Domain.BoundedContexts.UserManagement.Security.AggregateRoots.ApiClient.Behaviors;
using E_Commerce.Domain.SharedKernel.PersistenceAbstractions;

namespace Domain.BoundedContexts.UserManagement.Security.Repositories
{
    public interface IApiClientRepository : IRepository<ApiClient>
    {
        Task<ApiClient?> GetByClientIdAsync(string clientId, CancellationToken cancellationToken = default);
        Task<bool> ExistsByClientIdAsync(string clientId, CancellationToken cancellationToken = default);
    }
}
