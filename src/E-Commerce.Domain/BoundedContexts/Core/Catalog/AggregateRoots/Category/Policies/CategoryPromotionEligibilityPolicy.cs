using System;
using E_Commerce.Domain.BoundedContexts.CoreCommerce.Catalog.AggregateRoots.Category.Behaviors;

namespace E_Commerce.Domain.Catalog.AggregateRoots.Category.Policies
{
    public class CategoryPromotionEligibilityPolicy
    {
        public bool IsEligibleForPromotion(Category category, object promotion)
        {
            // Business Logic: Determine if products within this category are eligible for specific seasonal or targeted promotions.
            return true;
        }
    }
}
