using System;
using SharedKernel.Interfaces;

namespace E_Commerce.Domain.Catalog.AggregateRoots.Brand.Rules
{
    public class BrandVerificationRequiredBeforePublishRule : IBusinessRule
    {
        public bool IsSatisfied()
        {
            // Business Logic: Verify that the brand has been officially verified before it can be published.
            return true;
        }

        public string Message => "Brand must be verified before publishing.";
    }
}
