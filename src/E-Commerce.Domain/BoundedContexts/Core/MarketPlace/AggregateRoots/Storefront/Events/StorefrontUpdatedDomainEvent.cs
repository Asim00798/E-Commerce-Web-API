#if false
using E_Commerce.Domain.SharedKernel.Events;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.MarketPlace.AggregateRoots.Storefront.Events
{
    public class StorefrontUpdatedDomainEvent : DomainEvent
    {
        public Guid StorefrontId { get; }
        public string NewName { get; }

        public StorefrontUpdatedDomainEvent(Guid storefrontId, string newName)
        {
            StorefrontId = storefrontId;
            NewName = newName;
        }
    }
}

#endif