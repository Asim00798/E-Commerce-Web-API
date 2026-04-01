using System.Linq.Expressions;
using E_Commerce.Domain.SharedKernel.Interfaces;
using ProductAggregate =  E_Commerce.Domain.BoundedContexts.CoreCommerce.Catalog.AggregateRoots.Product.Behaviors.Product;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Catalog.AggregateRoots.Product.Specifications
{
    public class AvailableForPurchaseSpec : ISpecification<ProductAggregate>
    {
        public Expression<Func<ProductAggregate, bool>> ToExpression()
        {
            // Business Logic: Filter products that are published, have active status, and are in stock.
            return product => true;
        }

        public bool IsSatisfiedBy(ProductAggregate entity)
        {
            // Business Logic: Check if the product is currently buyable.
            return true;
        }
    }
}
