using E_Commerce.Domain.BoundedContexts.CoreCommerce.Catalog.AggregateRoots.Product.Exceptions;
using E_Commerce.Domain.BoundedContexts.CoreCommerce.Catalog.Enums; // For RestrictionType, RestrictionStatus
using E_Commerce.Domain.SharedKernel.Abstract;
using E_Commerce.Domain.SharedKernel.Interfaces;

namespace E_Commerce.Domain.ContextBounded.Catalog.AggregateRoots.Product.Entities
{
    public class ProductRestriction : BaseEntity, IEntity<ProductRestriction>
    {
        public RestrictionType Type { get; private set; }
        public string RestrictionValue { get; private set; } = null!; // e.g., country code, channel ID, price threshold
        public DateTime? EffectiveFrom { get; private set; }
        public DateTime? EffectiveTo { get; private set; }
        public RestrictionStatus Status { get; private set; }
        public int Priority { get; private set; } // lower number = higher priority
        public Guid ProductId { get; private set; }

        public static ProductRestriction Create(
            RestrictionType type,
            string restrictionValue,
            Guid productId,
            DateTime? effectiveFrom = null,
            DateTime? effectiveTo = null,
            int priority = 0)
        {
            // Validation
            if (string.IsNullOrWhiteSpace(restrictionValue))
                throw new ProductException("Restriction value is required.");
            if (effectiveFrom.HasValue && effectiveTo.HasValue && effectiveFrom >= effectiveTo)
                throw new ProductException("EffectiveFrom must be before EffectiveTo.");

            var restriction = new ProductRestriction
            {
                Id = Guid.NewGuid(),
                Type = type,
                RestrictionValue = restrictionValue.Trim(),
                EffectiveFrom = effectiveFrom,
                EffectiveTo = effectiveTo,
                Status = RestrictionStatus.Active,
                Priority = priority,
                ProductId = productId,
                CreatedAt = DateTime.UtcNow
            };

            return restriction;
        }

        public void UpdateDetails(
            string restrictionValue,
            DateTime? effectiveFrom,
            DateTime? effectiveTo,
            int priority,
            string updatedBy)
        {
            if (string.IsNullOrWhiteSpace(restrictionValue))
                throw new ProductException("Restriction value is required.");
            if (effectiveFrom.HasValue && effectiveTo.HasValue && effectiveFrom >= effectiveTo)
                throw new ProductException("EffectiveFrom must be before EffectiveTo.");

            RestrictionValue = restrictionValue.Trim();
            EffectiveFrom = effectiveFrom;
            EffectiveTo = effectiveTo;
            Priority = priority;
        }

        public void Activate(string activatedBy)
        {
            if (Status == RestrictionStatus.Active)
                return;
            Status = RestrictionStatus.Active;        
        }

        public void Deactivate(string reason, string deactivatedBy)
        {
            if (Status == RestrictionStatus.Inactive)
                return;
            Status = RestrictionStatus.Inactive;
        }

        public void Expire()
        {
            if (Status == RestrictionStatus.Expired)
                return;
            Status = RestrictionStatus.Expired;
        }

        public bool IsEffectiveAt(DateTime date)
        {
            if (Status != RestrictionStatus.Active)
                return false;
            if (EffectiveFrom.HasValue && date < EffectiveFrom.Value)
                return false;
            if (EffectiveTo.HasValue && date > EffectiveTo.Value)
                return false;
            return true;
        }
    }
}