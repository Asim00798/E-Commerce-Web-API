#if false
using E_Commerce.Domain.BoundedContexts.CoreCommerce.Pricing.AggregateRoots.Price.ValueObjects;
using E_Commerce.Domain.SharedKernel.Events;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Pricing.AggregateRoots.Price.Events
{
    public class PriceCalculatedDomainEvent : DomainEvent
    {
        public Guid PriceId { get; }
        public Money CalculatedPrice { get; }

        public PriceCalculatedDomainEvent(Guid priceId, Money calculatedPrice)
        {
            PriceId = priceId;
            CalculatedPrice = calculatedPrice;
        }
    }
}

#endif