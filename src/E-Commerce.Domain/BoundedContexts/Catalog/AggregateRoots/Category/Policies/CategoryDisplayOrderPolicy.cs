using System;
using E_Commerce.Domain.BoundedContexts.Catalog.AggregateRoots.Category.Behaviors;

namespace E_Commerce.Domain.Catalog.AggregateRoots.Category.Policies
{
    public class CategoryDisplayOrderPolicy
    {
        public int GetDisplayOrder(Category category)
        {
            // Business Logic: Calculate the sorting index for categories based on popularity, alphabetical order, or manual priority.
            return 0;
        }
    }
}
