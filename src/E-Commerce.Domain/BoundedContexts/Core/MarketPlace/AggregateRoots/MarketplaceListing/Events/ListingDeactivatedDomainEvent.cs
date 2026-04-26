#if false
using E_Commerce.Domain.SharedKernel.Events;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.MarketPlace.AggregateRoots.MarketplaceListing.Events
{
    public class ListingDeactivatedDomainEvent : DomainEvent
    {
        public Guid ListingId { get; }
        public string Reason { get; }

        public ListingDeactivatedDomainEvent(Guid listingId, string reason)
        {
            ListingId = listingId;
            Reason = reason;
        }
    }
}

#endif