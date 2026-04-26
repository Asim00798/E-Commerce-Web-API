#if false
using E_Commerce.Domain.BoundedContexts.CoreCommerce.Recommendation.AggregateRoots.ProductScore.ValueObjects;
using ProductScoreAggregate = E_Commerce.Domain.BoundedContexts.CoreCommerce.Recommendation.AggregateRoots.ProductScore.Behaviors.ProductScore;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Recommendation.AggregateRoots.ProductScore.Factories
{
    public static class ProductScoreFactory
    {
        public static ProductScoreAggregate Create(Guid productId, string type = "Popularity")
        {
            var pId = new ProductId(productId);
            var sType = new ScoreType(type);
            var window = TimeWindow.Past7Days;
            return new ProductScoreAggregate(pId, sType, window);
        }
    }
}

#endif