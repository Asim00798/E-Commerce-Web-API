#if false
using E_Commerce.Domain.BoundedContexts.CoreCommerce.Pricing.AggregateRoots.Price.ValueObjects;
using E_Commerce.Domain.SharedKernel.Entities;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Pricing.AggregateRoots.Price.Entities
{
    public class PriceHistoryEntry : BaseEntity
    {
        public Money OldPrice { get; private set; }
        public Money NewPrice { get; private set; }
        public string ChangeReason { get; private set; }
        public DateTime ChangedAt { get; private set; }

        public PriceHistoryEntry(Money oldPrice, Money newPrice, string changeReason)
        {
            OldPrice = oldPrice;
            NewPrice = newPrice;
            ChangeReason = changeReason;
            ChangedAt = DateTime.UtcNow;
        }
    }
}

#endif