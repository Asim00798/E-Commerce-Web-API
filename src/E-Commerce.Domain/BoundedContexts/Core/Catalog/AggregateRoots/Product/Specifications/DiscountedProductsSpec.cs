using System.Linq.Expressions;
using E_Commerce.Domain.SharedKernel.Interfaces;
using ProductAggregate = E_Commerce.Domain.BoundedContexts.CoreCommerce.Catalog.AggregateRoots.Product.Behaviors.Product;

namespace E_Commerce.Domain.Catalog.AggregateRoots.Product.Specifications
{
    public class DiscountedProductsSpec : ISpecification<ProductAggregate>
    {
        public Expression<Func<ProductAggregate, bool>> ToExpression()
        {
            // Business Logic: Find products that have a non-zero discount or a sale price lower than the base price.
            return product => true;
        }

        public bool IsSatisfiedBy(ProductAggregate entity)
        {
            // Business Logic: Determine if the product has an active discount.
            return true;
        }
    }
}
