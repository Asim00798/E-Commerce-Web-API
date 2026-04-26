#if false
using E_Commerce.Domain.BoundedContexts.CoreCommerce.Pricing.AggregateRoots.Price.Entities;
using E_Commerce.Domain.BoundedContexts.CoreCommerce.Pricing.AggregateRoots.Price.ValueObjects;
using E_Commerce.Domain.SharedKernel.Entities;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Pricing.AggregateRoots.Price.Behaviors
{
    public partial class Price : BaseEntity, IAggregateRoot
    {
        public Guid ProductId { get; private set; }
        public Money BasePrice { get; private set; }
        public Money CurrentPrice { get; private set; }
        
        private readonly List<PriceRule> _rules = new();
        public IReadOnlyCollection<PriceRule> Rules => _rules.AsReadOnly();

        public Price(Guid productId, Money basePrice)
        {
            ProductId = productId;
            BasePrice = basePrice;
            CurrentPrice = basePrice;
        }

        public void UpdatePrice(Money newBasePrice)
        {
            BasePrice = newBasePrice;
            // Additional logic for recalculating current price would go here
        }
    }
}

#endif