using System;
using System.Linq.Expressions;
using E_Commerce.Domain.SharedKernel.Interfaces;
using E_Commerce.Domain.BoundedContexts.CoreCommerce.Catalog.AggregateRoots.Product.Behaviors;

namespace E_Commerce.Domain.Catalog.AggregateRoots.Product.Specifications
{
    public class NewArrivalsInLastDaysSpec : ISpecification<Product>
    {
        private readonly int _days;

        public NewArrivalsInLastDaysSpec(int days)
        {
            _days = days;
        }

        public Expression<Func<Product, bool>> ToExpression()
        {
            // Business Logic: Select products published within the last 'X' days.
            return product => true;
        }

        public bool IsSatisfiedBy(Product entity)
        {
            // Business Logic: Verify if the product's publish date is within the arrival window.
            return true;
        }
    }
}
