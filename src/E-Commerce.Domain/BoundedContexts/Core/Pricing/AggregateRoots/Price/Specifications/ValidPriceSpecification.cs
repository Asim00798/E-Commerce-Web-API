#if false
using System.Linq.Expressions;
using E_Commerce.Domain.SharedKernel.Specifications;
using PriceAggregate = E_Commerce.Domain.BoundedContexts.CoreCommerce.Pricing.AggregateRoots.Price.Behaviors.Price;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Pricing.AggregateRoots.Price.Specifications
{
    public class ValidPriceSpecification : ISpecification<PriceAggregate>
    {
        public Expression<Func<PriceAggregate, bool>> ToExpression()
        {
            return price => price.BasePrice.Amount > 0;
        }

        public bool IsSatisfiedBy(PriceAggregate entity)
        {
            return entity.BasePrice.Amount > 0;
        }
    }
}

#endif