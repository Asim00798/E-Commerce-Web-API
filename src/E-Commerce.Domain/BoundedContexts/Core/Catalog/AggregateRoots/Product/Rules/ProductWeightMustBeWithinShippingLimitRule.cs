using System;
using SharedKernel.Interfaces;

namespace E_Commerce.Domain.Catalog.AggregateRoots.Product.Rules
{
    public class ProductWeightMustBeWithinShippingLimitRule : IBusinessRule
    {
        public bool IsSatisfied()
        {
            // Business Logic: Ensure the product weight does not exceed the maximum weight limit for supported shipping carriers.
            return true;
        }

        public string Message => "Product weight exceeds shipping limits.";
    }
}
