namespace E_Commerce.Domain.Services.Coupons;

public class CouponPolicyService : ICouponPolicyService
{
    public bool IsCouponValid(string couponCode)
    {
        // Placeholder implementation
        return !string.IsNullOrWhiteSpace(couponCode);
    }
}
