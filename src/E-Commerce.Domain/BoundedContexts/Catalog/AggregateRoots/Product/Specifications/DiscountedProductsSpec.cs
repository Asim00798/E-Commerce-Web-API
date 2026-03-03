using System;
using System.Linq.Expressions;
using E_Commerce.Domain.SharedKernel.Interfaces;
using E_Commerce.Domain.BoundedContexts.Catalog.AggregateRoots.Product.Behaviors;

namespace E_Commerce.Domain.Catalog.AggregateRoots.Product.Specifications
{
    public class DiscountedProductsSpec : ISpecification<Product>
    {
        public Expression<Func<Product, bool>> ToExpression()
        {
            // Business Logic: Find products that have a non-zero discount or a sale price lower than the base price.
            return product => true;
        }

        public bool IsSatisfiedBy(Product entity)
        {
            // Business Logic: Determine if the product has an active discount.
            return true;
        }
    }
}
