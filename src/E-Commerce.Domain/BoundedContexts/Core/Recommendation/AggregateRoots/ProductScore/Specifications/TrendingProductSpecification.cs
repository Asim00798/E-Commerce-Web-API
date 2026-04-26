#if false
using System.Linq.Expressions;
using E_Commerce.Domain.SharedKernel.Specifications;
using ProductScoreAggregate = E_Commerce.Domain.BoundedContexts.CoreCommerce.Recommendation.AggregateRoots.ProductScore.Behaviors.ProductScore;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Recommendation.AggregateRoots.ProductScore.Specifications
{
    public class TrendingProductSpecification : ISpecification<ProductScoreAggregate>
    {
        private readonly float _minTrendingScore;

        public TrendingProductSpecification(float minTrendingScore = 100f)
        {
            _minTrendingScore = minTrendingScore;
        }

        public Expression<Func<ProductScoreAggregate, bool>> ToExpression()
        {
            return score => score.TotalScore.Value >= _minTrendingScore;
        }

        public bool IsSatisfiedBy(ProductScoreAggregate entity)
        {
            return entity.TotalScore.Value >= _minTrendingScore;
        }
    }
}

#endif