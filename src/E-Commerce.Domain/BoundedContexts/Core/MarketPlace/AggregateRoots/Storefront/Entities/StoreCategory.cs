#if false
using E_Commerce.Domain.SharedKernel.Entities;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.MarketPlace.AggregateRoots.Storefront.Entities
{
    public class StoreCategory : BaseEntity
    {
        public string Name { get; private set; }
        public string Description { get; private set; }

        public StoreCategory(string name, string description)
        {
            Name = name;
            Description = description;
        }
    }
}

#endif