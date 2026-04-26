#if false
using E_Commerce.Domain.SharedKernel.Events;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.MarketPlace.AggregateRoots.Seller.Events
{
    public class SellerVerifiedDomainEvent : DomainEvent
    {
        public Guid SellerId { get; }
        public DateTime VerifiedAt { get; }

        public SellerVerifiedDomainEvent(Guid sellerId, DateTime verifiedAt)
        {
            SellerId = sellerId;
            VerifiedAt = verifiedAt;
        }
    }
}

#endif