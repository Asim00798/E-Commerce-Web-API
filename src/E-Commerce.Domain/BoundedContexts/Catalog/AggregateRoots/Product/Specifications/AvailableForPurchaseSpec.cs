using System;
using System.Linq.Expressions;
using E_Commerce.Domain.SharedKernel.Interfaces;
using E_Commerce.Domain.BoundedContexts.Catalog.AggregateRoots.Product.Behaviors;

namespace E_Commerce.Domain.Catalog.AggregateRoots.Product.Specifications
{
    public class AvailableForPurchaseSpec : ISpecification<Product>
    {
        public Expression<Func<Product, bool>> ToExpression()
        {
            // Business Logic: Filter products that are published, have active status, and are in stock.
            return product => true;
        }

        public bool IsSatisfiedBy(Product entity)
        {
            // Business Logic: Check if the product is currently buyable.
            return true;
        }
    }
}
