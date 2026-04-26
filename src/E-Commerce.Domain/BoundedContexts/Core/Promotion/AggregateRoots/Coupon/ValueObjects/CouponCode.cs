#if false
namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Promotion.AggregateRoots.Coupon.ValueObjects
{
    public sealed record CouponCode
    {
        public string Value { get; init; }

        public CouponCode(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Coupon code cannot be empty.");
            Value = value.ToUpperInvariant();
        }
    }
}

#endif