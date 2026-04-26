#if false
using E_Commerce.Domain.BoundedContexts.CoreCommerce.Pricing.AggregateRoots.Price.ValueObjects;
using E_Commerce.Domain.SharedKernel.Entities;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Pricing.AggregateRoots.Price.Entities
{
    public class PricingCampaign : BaseEntity
    {
        public string Name { get; private set; }
        public TimeRange ValidityPeriod { get; private set; }
        public decimal DiscountPercentage { get; private set; }
        public bool IsActive { get; private set; }

        public PricingCampaign(string name, TimeRange validityPeriod, decimal discountPercentage)
        {
            Name = name;
            ValidityPeriod = validityPeriod;
            DiscountPercentage = discountPercentage;
            IsActive = true;
        }

        public void Deactivate() => IsActive = false;
        
        public bool IsValidAt(DateTime dateTime) => IsActive && ValidityPeriod.IsActive(dateTime);
    }
}

#endif