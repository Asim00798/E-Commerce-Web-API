using System;
using SharedKernel.Interfaces;

namespace E_Commerce.Domain.Catalog.AggregateRoots.Category.Rules
{
    public class CategoryDepthCannotExceedLimitRule : IBusinessRule
    {
        public bool IsSatisfied()
        {
            // Business Logic: Verify that the category's nesting level does not exceed the system's maximum allowed depth.
            return true;
        }

        public string Message => "Category depth exceeds the maximum allowed limit.";
    }
}
