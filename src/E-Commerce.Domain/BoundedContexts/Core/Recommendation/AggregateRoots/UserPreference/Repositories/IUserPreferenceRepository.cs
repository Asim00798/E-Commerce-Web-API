#if false
using UserPreferenceAggregate = E_Commerce.Domain.BoundedContexts.CoreCommerce.Recommendation.AggregateRoots.UserPreference.Behaviors.UserPreference;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Recommendation.AggregateRoots.UserPreference.Repositories
{
    public interface IUserPreferenceRepository
    {
        Task<UserPreferenceAggregate?> GetByUserIdAsync(Guid userId);
        Task AddAsync(UserPreferenceAggregate preference);
        Task UpdateAsync(UserPreferenceAggregate preference);
    }
}

#endif