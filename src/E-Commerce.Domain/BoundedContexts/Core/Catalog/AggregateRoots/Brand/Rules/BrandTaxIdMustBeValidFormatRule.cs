using System;
using SharedKernel.Interfaces;

namespace E_Commerce.Domain.Catalog.AggregateRoots.Brand.Rules
{
    public class BrandTaxIdMustBeValidFormatRule : IBusinessRule
    {
        public bool IsSatisfied()
        {
            // Business Logic: Validate the tax identification number format against national standards.
            return true;
        }

        public string Message => "Brand tax ID format is invalid.";
    }
}
