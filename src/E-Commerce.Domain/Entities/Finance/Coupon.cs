using E_Commerce.Domain.Entities.Abstract;

namespace E_Commerce.Domain.Entities.Finance
{
    public class Coupon : BaseEntity
    {
        public string Code { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal DiscountAmount { get; set; } // Can be absolute or percentage
        public bool IsPercentage { get; set; } = false;
        public DateTimeOffset ValidFrom { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset ValidUntil { get; set; } = DateTimeOffset.UtcNow.AddMonths(1);
        public int UsageLimit { get; set; } = 1;
        public int TimesUsed { get; set; } = 0;
        public bool IsActive { get; set; } = true;

        // Navigation
        public ICollection<CouponUsage>? CouponUsages { get; set; }

        public override void Validate()
        {
            base.Validate();

            if (string.IsNullOrWhiteSpace(Code))
                throw new InvalidOperationException("Coupon code cannot be empty.");

            if (DiscountAmount <= 0)
                throw new InvalidOperationException("Discount amount must be greater than zero.");

            if (ValidUntil <= ValidFrom)
                throw new InvalidOperationException("Coupon ValidUntil must be after ValidFrom.");
        }
    }
}
