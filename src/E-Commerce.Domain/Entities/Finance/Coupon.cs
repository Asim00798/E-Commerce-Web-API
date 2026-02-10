using E_Commerce.Domain.Entities.Abstract;
using E_Commerce.Domain.Exceptions;
using E_Commerce.Domain.DomainEvents.Finance.Coupon;
using E_Commerce.Domain.DomainEvents.Finance.CouponUsage;

namespace E_Commerce.Domain.Entities.Finance
{
    public class Coupon : BaseEntity
    {
        public string Code { get; private set; } = string.Empty;
        public string? Description { get; private set; }
        public decimal DiscountAmount { get; private set; } // Can be absolute or percentage
        public bool IsPercentage { get; private set; } = false;
        public DateTimeOffset ValidFrom { get; private set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset ValidUntil { get; private set; } = DateTimeOffset.UtcNow.AddMonths(1);
        public int UsageLimit { get; private set; } = 1;
        public int TimesUsed { get; private set; } = 0;
        public bool IsActive { get; private set; } = true;

        // Navigation
        public ICollection<CouponUsage>? CouponUsages { get; private set; }

        // DDD Constructor
        public Coupon(string code, decimal discountAmount, bool isPercentage, DateTimeOffset validFrom, DateTimeOffset validUntil, int usageLimit)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new BusinessRuleViolationException("Coupon code cannot be empty.");

            if (discountAmount <= 0)
                throw new BusinessRuleViolationException("Discount amount must be greater than zero.");

            if (validUntil <= validFrom)
                throw new BusinessRuleViolationException("Coupon ValidUntil must be after ValidFrom.");

            Code = code;
            DiscountAmount = discountAmount;
            IsPercentage = isPercentage;
            ValidFrom = validFrom;
            ValidUntil = validUntil;
            UsageLimit = usageLimit;
            IsActive = true;

            AddDomainEvent(new CouponCreated(Id));
        }

        public void Activate()
        {
            if (IsActive) return;

            IsActive = true;
            AddDomainEvent(new CouponActivated(Id));
        }

        public void Deactivate()
        {
            if (!IsActive) return;

            IsActive = false;
            AddDomainEvent(new CouponDeactivated(Id));
        }

        public void Apply()
        {
            if (!IsActive)
                throw new BusinessRuleViolationException("Coupon is not active.");

            if (DateTimeOffset.UtcNow < ValidFrom || DateTimeOffset.UtcNow > ValidUntil)
            {
                AddDomainEvent(new CouponExpired(Id));
                throw new BusinessRuleViolationException("Coupon is expired or not yet valid.");
            }

            if (TimesUsed >= UsageLimit)
            {
                AddDomainEvent(new CouponUsageLimitReached(Id));
                throw new BusinessRuleViolationException("Coupon usage limit reached.");
            }

            TimesUsed++;
            AddDomainEvent(new CouponApplied(Id));
        }

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
