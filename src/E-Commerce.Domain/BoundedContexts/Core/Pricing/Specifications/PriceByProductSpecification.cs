#if false
using System.Linq.Expressions;
using E_Commerce.Domain.SharedKernel.Specifications;
using PriceAggregate = E_Commerce.Domain.BoundedContexts.CoreCommerce.Pricing.AggregateRoots.Price.Behaviors.Price;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Pricing.Specifications
{
    public class PriceByProductSpecification : ISpecification<PriceAggregate>
    {
        private readonly Guid _productId;

        public PriceByProductSpecification(Guid productId)
        {
            _productId = productId;
        }

        public Expression<Func<PriceAggregate, bool>> ToExpression()
        {
            return price => price.ProductId == _productId;
        }

        public bool IsSatisfiedBy(PriceAggregate entity)
        {
            return entity.ProductId == _productId;
        }
    }
}

#endif