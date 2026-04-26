#if false
using E_Commerce.Domain.BoundedContexts.CoreCommerce.Recommendation.AggregateRoots.RecommendationResult.ValueObjects;
using E_Commerce.Domain.BoundedContexts.CoreCommerce.Recommendation.Enums;
using RecommendationResultAggregate = E_Commerce.Domain.BoundedContexts.CoreCommerce.Recommendation.AggregateRoots.RecommendationResult.Behaviors.RecommendationResult;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Recommendation.AggregateRoots.RecommendationResult.Factories
{
    public static class RecommendationResultFactory
    {
        public static RecommendationResultAggregate Create(Guid? userId, RecommendationTypeEnum typeEnum = RecommendationTypeEnum.Personalized)
        {
            var recommendationId = RecommendationId.New();
            var type = new RecommendationType(typeEnum);
            return new RecommendationResultAggregate(recommendationId, userId, type);
        }
    }
}

#endif