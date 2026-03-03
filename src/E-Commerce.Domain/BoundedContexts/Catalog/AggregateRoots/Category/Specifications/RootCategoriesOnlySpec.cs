using System;
using System.Linq.Expressions;
using E_Commerce.Domain.SharedKernel.Interfaces;
using E_Commerce.Domain.BoundedContexts.Catalog.AggregateRoots.Category.Behaviors;

namespace E_Commerce.Domain.Catalog.AggregateRoots.Category.Specifications
{
    public class RootCategoriesOnlySpec : ISpecification<Category>
    {
        public Expression<Func<Category, bool>> ToExpression()
        {
            // Business Logic: Select categories that have no parent category defined (top-level nodes).
            return category => true;
        }

        public bool IsSatisfiedBy(Category entity)
        {
            // Business Logic: Check if the category is a root level entity.
            return true;
        }
    }
}
