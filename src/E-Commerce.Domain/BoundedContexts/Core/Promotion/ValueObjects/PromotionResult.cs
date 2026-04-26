#if false
using E_Commerce.Domain.BoundedContexts.CoreCommerce.Promotion.ValueObjects;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Promotion.ValueObjects
{
    public sealed record PromotionResult
    {
        public bool IsEligible { get; init; }
        public string Message { get; init; }
        public DiscountDescriptor? AppliedDiscount { get; init; }

        public PromotionResult(bool isEligible, string message, DiscountDescriptor? appliedDiscount = null)
        {
            IsEligible = isEligible;
            Message = message;
            AppliedDiscount = appliedDiscount;
        }

        public static PromotionResult Ineligible(string message) => new(false, message);
        public static PromotionResult Eligible(DiscountDescriptor discount) => new(true, "Promotion Applied", discount);
    }
}

#endif