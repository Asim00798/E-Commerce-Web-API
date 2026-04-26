#if false
using RecommendationModelAggregate = E_Commerce.Domain.BoundedContexts.CoreCommerce.Recommendation.AggregateRoots.RecommendationModel.Behaviors.RecommendationModel;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Recommendation.AggregateRoots.RecommendationModel.Repositories
{
    public interface IRecommendationModelRepository
    {
        Task<RecommendationModelAggregate?> GetByIdAsync(Guid id);
        Task<IEnumerable<RecommendationModelAggregate>> GetAllActiveAsync();
        Task AddAsync(RecommendationModelAggregate model);
        Task UpdateAsync(RecommendationModelAggregate model);
    }
}

#endif