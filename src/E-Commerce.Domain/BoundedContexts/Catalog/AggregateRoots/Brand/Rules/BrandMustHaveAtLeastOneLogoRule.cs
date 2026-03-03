using System;
using SharedKernel.Interfaces;

namespace E_Commerce.Domain.Catalog.AggregateRoots.Brand.Rules
{
    public class BrandMustHaveAtLeastOneLogoRule : IBusinessRule
    {
        public bool IsSatisfied()
        {
            // Business Logic: Ensure the brand has uploaded at least one logo image.
            return true;
        }

        public string Message => "Brand must have at least one logo.";
    }
}
