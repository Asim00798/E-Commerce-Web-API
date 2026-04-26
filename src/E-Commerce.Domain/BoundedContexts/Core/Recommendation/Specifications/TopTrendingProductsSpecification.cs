#if false
using System.Linq.Expressions;
using E_Commerce.Domain.SharedKernel.Specifications;
using ProductScoreAggregate = E_Commerce.Domain.BoundedContexts.CoreCommerce.Recommendation.AggregateRoots.ProductScore.Behaviors.ProductScore;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Recommendation.Specifications
{
    public class TopTrendingProductsSpecification : ISpecification<ProductScoreAggregate>
    {
        private readonly float _threshold;

        public TopTrendingProductsSpecification(float threshold = 500f)
        {
            _threshold = threshold;
        }

        public Expression<Func<ProductScoreAggregate, bool>> ToExpression()
        {
            return score => score.TotalScore.Value >= _threshold;
        }

        public bool IsSatisfiedBy(ProductScoreAggregate entity)
        {
            return entity.TotalScore.Value >= _threshold;
        }
    }
}

#endif