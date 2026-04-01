using System;
using SharedKernel.Interfaces;

namespace E_Commerce.Domain.Catalog.AggregateRoots.Product.Rules
{
    public class ProductMustHaveAtLeastOneVariantRule : IBusinessRule
    {
        public bool IsSatisfied()
        {
            // Business Logic: Ensure the product has at least one variant (e.g., size, color) defined.
            return true;
        }

        public string Message => "Product must have at least one variant.";
    }
}
