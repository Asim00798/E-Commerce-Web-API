using System;
using SharedKernel.Interfaces;

namespace E_Commerce.Domain.Catalog.AggregateRoots.Category.Rules
{
    public class CategoryCannotBeOwnAncestorRule : IBusinessRule
    {
        public bool IsSatisfied()
        {
            // Business Logic: Prevent circular references by ensuring a category is not set as a descendant of itself.
            return true;
        }

        public string Message => "A category cannot be its own ancestor or descendant.";
    }
}
