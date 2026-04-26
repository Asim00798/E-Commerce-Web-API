#if false
using System.Linq.Expressions;
using E_Commerce.Domain.SharedKernel.Specifications;
using RecommendationResultAggregate = E_Commerce.Domain.BoundedContexts.CoreCommerce.Recommendation.AggregateRoots.RecommendationResult.Behaviors.RecommendationResult;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Recommendation.Specifications
{
    public class PersonalizedRecommendationSpecification : ISpecification<RecommendationResultAggregate>
    {
        private readonly Guid _userId;

        public PersonalizedRecommendationSpecification(Guid userId)
        {
            _userId = userId;
        }

        public Expression<Func<RecommendationResultAggregate, bool>> ToExpression()
        {
            return result => result.UserId == _userId;
        }

        public bool IsSatisfiedBy(RecommendationResultAggregate entity)
        {
            return entity.UserId == _userId;
        }
    }
}

#endif