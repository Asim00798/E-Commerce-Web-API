#if false
using E_Commerce.Domain.BoundedContexts.CoreCommerce.Pricing.Enums;
using E_Commerce.Domain.SharedKernel.Entities;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Pricing.AggregateRoots.Price.Entities
{
    public class PriceRule : BaseEntity
    {
        public string Name { get; private set; }
        public PricingStrategyType StrategyType { get; private set; }
        public decimal Value { get; private set; }
        public bool IsActive { get; private set; }

        public PriceRule(string name, PricingStrategyType strategyType, decimal value)
        {
            Name = name;
            StrategyType = strategyType;
            Value = value;
            IsActive = true;
        }

        public void Deactivate() => IsActive = false;
        public void Activate() => IsActive = true;
    }
}

#endif