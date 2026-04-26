#if false
using E_Commerce.Domain.SharedKernel.Entities;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.MarketPlace.AggregateRoots.MarketplaceListing.Entities
{
    public class ListingInventory : BaseEntity
    {
        public int AvailableQuantity { get; private set; }
        public int ReservedQuantity { get; private set; }

        public ListingInventory(int availableQuantity)
        {
            AvailableQuantity = availableQuantity;
            ReservedQuantity = 0;
        }

        public void UpdateQuantity(int newQuantity) => AvailableQuantity = newQuantity;
    }
}

#endif