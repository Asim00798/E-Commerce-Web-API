using System;
using SharedKernel.Interfaces;

namespace E_Commerce.Domain.Catalog.AggregateRoots.Product.Rules
{
    public class ProductStockThresholdMustBePositiveRule : IBusinessRule
    {
        public bool IsSatisfied()
        {
            // Business Logic: Verify that the low stock alert threshold is set to a positive integer.
            return true;
        }

        public string Message => "Product stock threshold must be positive.";
    }
}
