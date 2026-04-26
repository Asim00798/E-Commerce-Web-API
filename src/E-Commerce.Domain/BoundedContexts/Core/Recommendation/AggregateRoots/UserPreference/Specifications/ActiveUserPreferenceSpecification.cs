#if false
using System.Linq.Expressions;
using E_Commerce.Domain.SharedKernel.Specifications;
using UserPreferenceAggregate = E_Commerce.Domain.BoundedContexts.CoreCommerce.Recommendation.AggregateRoots.UserPreference.Behaviors.UserPreference;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Recommendation.AggregateRoots.UserPreference.Specifications
{
    public class ActiveUserPreferenceSpecification : ISpecification<UserPreferenceAggregate>
    {
        public Expression<Func<UserPreferenceAggregate, bool>> ToExpression()
        {
            return pref => pref.Interactions.Any(i => i.InteractionTime >= DateTime.UtcNow.AddDays(-30));
        }

        public bool IsSatisfiedBy(UserPreferenceAggregate entity)
        {
            return entity.Interactions.Any(i => i.InteractionTime >= DateTime.UtcNow.AddDays(-30));
        }
    }
}

#endif