#if false
using ProductScoreAggregate = E_Commerce.Domain.BoundedContexts.CoreCommerce.Recommendation.AggregateRoots.ProductScore.Behaviors.ProductScore;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Recommendation.AggregateRoots.ProductScore.Repositories
{
    public interface IProductScoreRepository
    {
        Task<ProductScoreAggregate?> GetByProductIdAsync(Guid productId);
        Task<IEnumerable<ProductScoreAggregate>> GetTopTrendingAsync(int count);
        Task AddAsync(ProductScoreAggregate score);
        Task UpdateAsync(ProductScoreAggregate score);
    }
}

#endif