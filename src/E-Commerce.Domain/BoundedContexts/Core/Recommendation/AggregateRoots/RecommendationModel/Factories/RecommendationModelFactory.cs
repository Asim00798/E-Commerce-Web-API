#if false
using E_Commerce.Domain.BoundedContexts.CoreCommerce.Recommendation.AggregateRoots.RecommendationModel.ValueObjects;
using E_Commerce.Domain.BoundedContexts.CoreCommerce.Recommendation.Enums;
using RecommendationModelAggregate = E_Commerce.Domain.BoundedContexts.CoreCommerce.Recommendation.AggregateRoots.RecommendationModel.Behaviors.RecommendationModel;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Recommendation.AggregateRoots.RecommendationModel.Factories
{
    public static class RecommendationModelFactory
    {
        public static RecommendationModelAggregate Create(string name, ModelTypeEnum typeEnum = ModelTypeEnum.CollaborativeFiltering)
        {
            var modelType = new ModelType(typeEnum);
            return new RecommendationModelAggregate(name, modelType);
        }
    }
}

#endif