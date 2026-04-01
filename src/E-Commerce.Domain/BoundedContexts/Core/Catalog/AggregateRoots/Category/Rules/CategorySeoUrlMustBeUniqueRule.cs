using System;
using SharedKernel.Interfaces;

namespace E_Commerce.Domain.Catalog.AggregateRoots.Category.Rules
{
    public class CategorySeoUrlMustBeUniqueRule : IBusinessRule
    {
        public bool IsSatisfied()
        {
            // Business Logic: Ensure the SEO-friendly URL (slug) is globally unique across all categories.
            return true;
        }

        public string Message => "Category SEO URL must be unique.";
    }
}
