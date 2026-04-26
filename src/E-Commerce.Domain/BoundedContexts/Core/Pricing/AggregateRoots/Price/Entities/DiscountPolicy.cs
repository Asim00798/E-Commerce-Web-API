#if false
using E_Commerce.Domain.BoundedContexts.CoreCommerce.Pricing.Enums;
using E_Commerce.Domain.SharedKernel.Entities;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Pricing.AggregateRoots.Price.Entities
{
    public class DiscountPolicy : BaseEntity
    {
        public string Name { get; private set; }
        public DiscountType Type { get; private set; }
        public decimal Value { get; private set; }
        public int MinimumQuantity { get; private set; }
        public bool Stackable { get; private set; }

        public DiscountPolicy(string name, DiscountType type, decimal value, int minimumQuantity = 1, bool stackable = true)
        {
            Name = name;
            Type = type;
            Value = value;
            MinimumQuantity = minimumQuantity;
            Stackable = stackable;
        }
    }
}

#endif