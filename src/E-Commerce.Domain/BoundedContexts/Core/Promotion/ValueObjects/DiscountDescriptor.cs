#if false
using E_Commerce.Domain.BoundedContexts.CoreCommerce.Promotion.Enums;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Promotion.ValueObjects
{
    public sealed record DiscountDescriptor
    {
        public PromotionTypeEnum Type { get; init; }
        public decimal Value { get; init; }
        public string Description { get; init; }

        public DiscountDescriptor(PromotionTypeEnum type, decimal value, string description)
        {
            Type = type;
            Value = value;
            Description = description;
        }
    }
}

#endif