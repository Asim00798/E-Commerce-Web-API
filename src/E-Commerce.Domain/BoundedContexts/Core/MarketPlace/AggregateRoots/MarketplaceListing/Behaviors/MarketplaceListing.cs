#if false
using E_Commerce.Domain.BoundedContexts.CoreCommerce.MarketPlace.AggregateRoots.MarketplaceListing.Entities;
using E_Commerce.Domain.BoundedContexts.CoreCommerce.MarketPlace.AggregateRoots.MarketplaceListing.ValueObjects;
using E_Commerce.Domain.BoundedContexts.CoreCommerce.MarketPlace.AggregateRoots.Seller.ValueObjects;
using E_Commerce.Domain.SharedKernel.Entities;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.MarketPlace.AggregateRoots.MarketplaceListing.Behaviors
{
    public partial class MarketplaceListing : BaseEntity, IAggregateRoot
    {
        public SellerId SellerId { get; private set; }
        public ProductId ProductId { get; private set; }
        public PriceId PriceId { get; private set; }
        public ListingStatus Status { get; private set; }
        
        public ListingInventory Inventory { get; private set; }
        public ListingVisibility Visibility { get; private set; }
        public ListingModeration Moderation { get; private set; }

        public MarketplaceListing(SellerId sellerId, ProductId productId, PriceId priceId)
        {
            SellerId = sellerId;
            ProductId = productId;
            PriceId = priceId;
            Status = ListingStatus.Draft;
            
            Inventory = new ListingInventory(0);
            Visibility = new ListingVisibility(false);
            Moderation = new ListingModeration();
        }

        public void Activate() => Status = ListingStatus.Active;
        public void Deactivate() => Status = ListingStatus.Deactivated;
    }
}

#endif