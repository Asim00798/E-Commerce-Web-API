using E_Commerce.Domain.Entities.Abstract;
using E_Commerce.Domain.Exceptions;
using E_Commerce.Domain.DomainEvents.Finance.Coupon;
using E_Commerce.Domain.DomainEvents.Finance.CouponUsage;
using E_Commerce.Domain.ValueObjects;

namespace E_Commerce.Domain.Entities.Finance
{
    public class Coupon : BaseEntity
    {
        public string Code { get; private set; } = string.Empty;
        public string? Description { get; private set; }
        public Money DiscountAmount { get; private set; } 
        public bool IsPercentage { get; private set; }
        public DateTimeOffset ValidFrom { get; private set; }
        public DateTimeOffset ValidUntil { get; private set; }
        public int UsageLimit { get; private set; }
        public int TimesUsed { get; private set; } = 0;
        public bool IsActive { get; private set; } = true;

        // Navigation
        public ICollection<CouponUsage>? CouponUsages { get; private set; }

        // ----------------------
        // DDD Constructor
        // ----------------------
        public Coupon(string code, decimal discountAmount, bool isPercentage,
                      DateTimeOffset validFrom, DateTimeOffset validUntil, int usageLimit)
        {
            Code = ValidateCode(code);
            DiscountAmount = new Money(ValidateDiscount(discountAmount)) ;
            IsPercentage = isPercentage;
            ValidFrom = validFrom;
            ValidUntil = ValidateDates(validFrom, validUntil);
            UsageLimit = ValidateUsageLimit(usageLimit);
            IsActive = true;

            AddDomainEvent(new CouponCreated(Id));
        }

        // ----------------------
        // Behavior methods
        // ----------------------
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
            EnsureActive();
            EnsureValidDate();
            EnsureUsageLimitNotExceeded();

            TimesUsed++;
            AddDomainEvent(new CouponApplied(Id));
        }

        // ----------------------
        // Validation helpers
        // ----------------------
        private string ValidateCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new BusinessRuleViolationException("Coupon code cannot be empty.");
            return code.Trim();
        }

        private decimal ValidateDiscount(decimal amount)
        {
            if (amount <= 0)
                throw new BusinessRuleViolationException("Discount amount must be greater than zero.");
            return amount;
        }

        private DateTimeOffset ValidateDates(DateTimeOffset from, DateTimeOffset until)
        {
            if (until <= from)
                throw new BusinessRuleViolationException("Coupon ValidUntil must be after ValidFrom.");
            return until;
        }

        private int ValidateUsageLimit(int limit)
        {
            if (limit <= 0)
                throw new BusinessRuleViolationException("Usage limit must be greater than zero.");
            return limit;
        }

        private void EnsureActive()
        {
            if (!IsActive)
                throw new BusinessRuleViolationException("Coupon is not active.");
        }

        private void EnsureValidDate()
        {
            var now = DateTimeOffset.UtcNow;
            if (now < ValidFrom || now > ValidUntil)
            {
                AddDomainEvent(new CouponExpired(Id));
                throw new BusinessRuleViolationException("Coupon is expired or not yet valid.");
            }
        }

        private void EnsureUsageLimitNotExceeded()
        {
            if (TimesUsed >= UsageLimit)
            {
                AddDomainEvent(new CouponUsageLimitReached(Id));
                throw new BusinessRuleViolationException("Coupon usage limit reached.");
            }
        }

        // ----------------------
        // Domain validation (for EF / consistency)
        // ----------------------
        public override void Validate()
        {
            base.Validate();
            ValidateCode(Code);
            ValidateDiscount(DiscountAmount.Amount);
            ValidateDates(ValidFrom, ValidUntil);
            ValidateUsageLimit(UsageLimit);
        }
    }
}
