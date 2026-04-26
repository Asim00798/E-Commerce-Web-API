#if false
using E_Commerce.Domain.BoundedContexts.CoreCommerce.Promotion.AggregateRoots.PromotionPolicy.ValueObjects;
using PromotionPolicyAggregate = E_Commerce.Domain.BoundedContexts.CoreCommerce.Promotion.AggregateRoots.PromotionPolicy.Behaviors.PromotionPolicy;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Promotion.AggregateRoots.PromotionPolicy.Factories
{
    public static class PromotionPolicyFactory
    {
        public static PromotionPolicyAggregate Create(string name, string type = "Global")
        {
            var policyType = new PolicyType(type);
            return new PromotionPolicyAggregate(name, policyType);
        }
    }
}

#endif