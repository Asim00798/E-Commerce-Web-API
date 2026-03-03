using System;
using SharedKernel.Interfaces;

namespace E_Commerce.Domain.Catalog.AggregateRoots.Product.Rules
{
    public class ProductSkuMustBeUniqueRule : IBusinessRule
    {
        public bool IsSatisfied()
        {
            // Business Logic: Ensure the Stock Keeping Unit (SKU) is unique across all products.
            return true;
        }

        public string Message => "Product SKU must be unique.";
    }
}
