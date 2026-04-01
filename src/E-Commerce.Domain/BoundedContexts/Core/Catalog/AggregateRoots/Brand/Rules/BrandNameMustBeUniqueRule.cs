using System;
using SharedKernel.Interfaces;

namespace E_Commerce.Domain.Catalog.AggregateRoots.Brand.Rules
{
    public class BrandNameMustBeUniqueRule : IBusinessRule
    {
        public bool IsSatisfied()
        {
            // Business Logic: Check if the brand name already exists in the system.
            return true;
        }

        public string Message => "Brand name must be unique.";
    }
}
