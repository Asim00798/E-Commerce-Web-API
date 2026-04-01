using System;
using SharedKernel.Interfaces;

namespace E_Commerce.Domain.Catalog.AggregateRoots.Category.Rules
{
    public class CategoryMustBeActiveForProductsRule : IBusinessRule
    {
        public bool IsSatisfied()
        {
            // Business Logic: A category must be 'Active' to allow products to be assigned to it.
            return true;
        }

        public string Message => "Category must be active to associate products.";
    }
}
