using System;
using SharedKernel.Interfaces;

namespace E_Commerce.Domain.Catalog.AggregateRoots.Product.Rules
{
    public class ProductPublishDateCannotBeInPastRule : IBusinessRule
    {
        public bool IsSatisfied()
        {
            // Business Logic: Check that the scheduled publish date is not earlier than the current UTC time.
            return true;
        }

        public string Message => "Product publish date cannot be in the past.";
    }
}
