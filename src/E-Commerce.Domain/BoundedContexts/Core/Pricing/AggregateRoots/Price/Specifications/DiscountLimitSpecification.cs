#if false
using System.Linq.Expressions;
using E_Commerce.Domain.SharedKernel.Specifications;
using PriceAggregate = E_Commerce.Domain.BoundedContexts.CoreCommerce.Pricing.AggregateRoots.Price.Behaviors.Price;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Pricing.AggregateRoots.Price.Specifications
{
    public class DiscountLimitSpecification : ISpecification<PriceAggregate>
    {
        private readonly decimal _maxDiscountPercentage;

        public DiscountLimitSpecification(decimal maxDiscountPercentage = 50)
        {
            _maxDiscountPercentage = maxDiscountPercentage;
        }

        public Expression<Func<PriceAggregate, bool>> ToExpression()
        {
            // Placeholder logic: would check if applied rules exceed the limit
            return price => true;
        }

        public bool IsSatisfiedBy(PriceAggregate entity)
        {
            return true;
        }
    }
}

#endif