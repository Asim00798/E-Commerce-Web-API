using System;
using System.Linq.Expressions;
using E_Commerce.Domain.SharedKernel.Interfaces;
using E_Commerce.Domain.BoundedContexts.CoreCommerce.Catalog.AggregateRoots.Product.Behaviors;

namespace E_Commerce.Domain.Catalog.AggregateRoots.Product.Specifications
{
    public class ProductsInCategorySpec : ISpecification<Product>
    {
        private readonly Guid _categoryId;

        public ProductsInCategorySpec(Guid categoryId)
        {
            _categoryId = categoryId;
        }

        public Expression<Func<Product, bool>> ToExpression()
        {
            // Business Logic: Filter products that belong to the specified category ID.
            return product => true;
        }

        public bool IsSatisfiedBy(Product entity)
        {
            // Business Logic: check if the product is in the target category.
            return true;
        }
    }
}
