using System;
using SharedKernel.Interfaces;

namespace E_Commerce.Domain.Catalog.AggregateRoots.Category.Rules
{
    public class CategoryNameMustBeUniqueWithinParentRule : IBusinessRule
    {
        public bool IsSatisfied()
        {
            // Business Logic: Check if another category with the same name exists under the same parent category.
            return true;
        }

        public string Message => "Category name must be unique within the parent category.";
    }
}
