#if false
using E_Commerce.Domain.SharedKernel.Events;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.MarketPlace.AggregateRoots.Seller.Events
{
    public class SellerSuspendedDomainEvent : DomainEvent
    {
        public Guid SellerId { get; }
        public string Reason { get; }

        public SellerSuspendedDomainEvent(Guid sellerId, string reason)
        {
            SellerId = sellerId;
            Reason = reason;
        }
    }
}

#endif