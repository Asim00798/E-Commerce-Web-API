#if false
using E_Commerce.Domain.BoundedContexts.CoreCommerce.Pricing.Enums;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Pricing.AggregateRoots.Price.ValueObjects
{
    public sealed record Discount
    {
        public decimal Value { get; init; }
        public DiscountType Type { get; init; }

        public Discount(decimal value, DiscountType type)
        {
            Value = value;
            Type = type;
        }

        public static Discount None => new(0, DiscountType.None);
        public static Discount Percentage(decimal percentage) => new(percentage, DiscountType.Percentage);
        public static Discount Fixed(decimal amount) => new(amount, DiscountType.FixedAmount);
    }
}

#endif