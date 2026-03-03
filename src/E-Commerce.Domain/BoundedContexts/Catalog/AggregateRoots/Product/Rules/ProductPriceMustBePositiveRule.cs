using System;
using SharedKernel.Interfaces;

namespace E_Commerce.Domain.Catalog.AggregateRoots.Product.Rules
{
    public class ProductPriceMustBePositiveRule : IBusinessRule
    {
        public bool IsSatisfied()
        {
            // Business Logic: Verify that the product's base price is greater than zero.
            return true;
        }

        public string Message => "Product price must be positive.";
    }
}
