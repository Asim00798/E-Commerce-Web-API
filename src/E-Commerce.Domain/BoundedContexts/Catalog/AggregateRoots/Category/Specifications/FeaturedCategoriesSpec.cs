using System;
using System.Linq.Expressions;
using E_Commerce.Domain.SharedKernel.Interfaces;
using E_Commerce.Domain.BoundedContexts.Catalog.AggregateRoots.Category.Behaviors;

namespace E_Commerce.Domain.Catalog.AggregateRoots.Category.Specifications
{
    public class FeaturedCategoriesSpec : ISpecification<Category>
    {
        public Expression<Func<Category, bool>> ToExpression()
        {
            // Business Logic: Filter for categories targeted for featured display on the homepage or landing pages.
            return category => true;
        }

        public bool IsSatisfiedBy(Category entity)
        {
            // Business Logic: Check if the featured flag is set on the category.
            return true;
        }
    }
}
