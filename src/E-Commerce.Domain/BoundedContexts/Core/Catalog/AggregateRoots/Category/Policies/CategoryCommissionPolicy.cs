using System;
using E_Commerce.Domain.BoundedContexts.CoreCommerce.Catalog.AggregateRoots.Category.Behaviors;

namespace E_Commerce.Domain.Catalog.AggregateRoots.Category.Policies
{
    public class CategoryCommissionPolicy
    {
        public decimal GetCommissionRate(Category category)
        {
            // Business Logic: Determine commission rates specific to the product category (e.g., higher for electronics, lower for groceries).
            return 0.05m;
        }
    }
}
