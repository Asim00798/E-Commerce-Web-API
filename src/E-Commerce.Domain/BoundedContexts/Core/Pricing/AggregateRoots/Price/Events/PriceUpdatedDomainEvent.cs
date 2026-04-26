#if false
using E_Commerce.Domain.BoundedContexts.CoreCommerce.Pricing.AggregateRoots.Price.ValueObjects;
using E_Commerce.Domain.SharedKernel.Events;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Pricing.AggregateRoots.Price.Events
{
    public class PriceUpdatedDomainEvent : DomainEvent
    {
        public Guid PriceId { get; }
        public Money NewPrice { get; }

        public PriceUpdatedDomainEvent(Guid priceId, Money newPrice)
        {
            PriceId = priceId;
            NewPrice = newPrice;
        }
    }
}

#endif