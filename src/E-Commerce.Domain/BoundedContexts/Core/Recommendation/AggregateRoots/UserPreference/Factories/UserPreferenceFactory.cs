#if false
using E_Commerce.Domain.BoundedContexts.CoreCommerce.Recommendation.AggregateRoots.UserPreference.ValueObjects;
using UserPreferenceAggregate = E_Commerce.Domain.BoundedContexts.CoreCommerce.Recommendation.AggregateRoots.UserPreference.Behaviors.UserPreference;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Recommendation.AggregateRoots.UserPreference.Factories
{
    public static class UserPreferenceFactory
    {
        public static UserPreferenceAggregate Create(Guid userId)
        {
            var uId = new UserId(userId);
            return new UserPreferenceAggregate(uId);
        }
    }
}

#endif