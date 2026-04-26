#if false
using E_Commerce.Domain.SharedKernel.Events;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.MarketPlace.AggregateRoots.Seller.Events
{
    public class SellerOnboardedDomainEvent : DomainEvent
    {
        public Guid SellerId { get; }
        public string SellerName { get; }

        public SellerOnboardedDomainEvent(Guid sellerId, string sellerName)
        {
            SellerId = sellerId;
            SellerName = sellerName;
        }
    }
}

#endif