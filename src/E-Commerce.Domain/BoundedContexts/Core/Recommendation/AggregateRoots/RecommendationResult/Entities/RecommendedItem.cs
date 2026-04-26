#if false
using E_Commerce.Domain.BoundedContexts.CoreCommerce.Recommendation.AggregateRoots.RecommendationResult.ValueObjects;
using E_Commerce.Domain.SharedKernel.Entities;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Recommendation.AggregateRoots.RecommendationResult.Entities
{
    public class RecommendedItem : BaseEntity
    {
        public Guid ProductId { get; private set; }
        public RankingScore Score { get; private set; }

        public RecommendedItem(Guid productId, RankingScore score)
        {
            ProductId = productId;
            Score = score;
        }
    }
}

#endif