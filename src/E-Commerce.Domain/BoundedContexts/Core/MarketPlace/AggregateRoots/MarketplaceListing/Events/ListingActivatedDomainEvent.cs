#if false
using E_Commerce.Domain.SharedKernel.Events;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.MarketPlace.AggregateRoots.MarketplaceListing.Events
{
    public class ListingActivatedDomainEvent : DomainEvent
    {
        public Guid ListingId { get; }

        public ListingActivatedDomainEvent(Guid listingId)
        {
            ListingId = listingId;
        }
    }
}

#endif