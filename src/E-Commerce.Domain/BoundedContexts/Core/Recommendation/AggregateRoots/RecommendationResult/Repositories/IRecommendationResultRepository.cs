#if false
using RecommendationResultAggregate = E_Commerce.Domain.BoundedContexts.CoreCommerce.Recommendation.AggregateRoots.RecommendationResult.Behaviors.RecommendationResult;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Recommendation.AggregateRoots.RecommendationResult.Repositories
{
    public interface IRecommendationResultRepository
    {
        Task<RecommendationResultAggregate?> GetByIdAsync(Guid id);
        Task<IEnumerable<RecommendationResultAggregate>> GetByUserIdAsync(Guid userId);
        Task AddAsync(RecommendationResultAggregate result);
    }
}

#endif