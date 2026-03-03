using E_Commerce.Domain.ContextBounded.Catalog.AggregateRoots.Product.Exceptions;
using E_Commerce.Domain.ContextBounded.Catalog.Enums; // For RestrictionType, RestrictionStatus
using E_Commerce.Domain.SharedKernel.Abstract;

namespace E_Commerce.Domain.ContextBounded.Catalog.AggregateRoots.Product.Entities
{
    public class ProductRestriction : BaseEntity
    {
        public RestrictionType Type { get; private set; }
        public string RestrictionValue { get; private set; }
        public DateTime? EffectiveFrom { get; private set; }
        public DateTime? EffectiveTo { get; private set; }
        public RestrictionStatus Status { get; private set; }
        public int Priority { get; private set; } // lower number = higher priority
        public Guid ProductId { get; private set; }

        private ProductRestriction() { } // For EF Core

        public static ProductRestriction Create(
            RestrictionType type,
            string restrictionValue,
            Guid productId,
            string createdBy,
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
                CreatedAt = DateTime.UtcNow,
                CreatedBy = createdBy
            };

            restriction.AddDomainEvent(new ProductRestrictionCreated(
                restriction.Id, productId, type, restrictionValue, createdBy));
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
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = updatedBy;

            AddDomainEvent(new ProductRestrictionUpdated(Id, ProductId, updatedBy));
        }

        public void Activate(string activatedBy)
        {
            if (Status == RestrictionStatus.Active)
                return;
            Status = RestrictionStatus.Active;
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = activatedBy;
            AddDomainEvent(new ProductRestrictionActivated(Id, ProductId, activatedBy));
        }

        public void Deactivate(string reason, string deactivatedBy)
        {
            if (Status == RestrictionStatus.Inactive)
                return;
            Status = RestrictionStatus.Inactive;
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = deactivatedBy;
            AddDomainEvent(new ProductRestrictionDeactivated(Id, ProductId, reason, deactivatedBy));
        }

        public void Expire()
        {
            if (Status == RestrictionStatus.Expired)
                return;
            Status = RestrictionStatus.Expired;
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = "System";
            AddDomainEvent(new ProductRestrictionExpired(Id, ProductId));
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