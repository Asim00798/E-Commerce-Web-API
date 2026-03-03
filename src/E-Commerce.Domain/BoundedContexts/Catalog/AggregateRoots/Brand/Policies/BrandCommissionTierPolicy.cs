using System;

namespace E_Commerce.Domain.Catalog.AggregateRoots.Brand.Policies
{
    public class BrandCommissionTierPolicy
    {
        public decimal GetCommissionRate(decimal saleAmount)
        {
            // Business Logic: Determine the commission rate based on the brand's total sales volume or individual sale amount.
            return 0.10m; // Default 10%
        }
    }
}
