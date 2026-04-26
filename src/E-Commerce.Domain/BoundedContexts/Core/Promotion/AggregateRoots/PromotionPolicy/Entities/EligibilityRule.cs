#if false
using E_Commerce.Domain.SharedKernel.Entities;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Promotion.AggregateRoots.PromotionPolicy.Entities
{
    public class EligibilityRule : BaseEntity
    {
        public string PropertyName { get; private set; }
        public string ExpectedValue { get; private set; }

        public EligibilityRule(string propertyName, string expectedValue)
        {
            PropertyName = propertyName;
            ExpectedValue = expectedValue;
        }
    }
}

#endif