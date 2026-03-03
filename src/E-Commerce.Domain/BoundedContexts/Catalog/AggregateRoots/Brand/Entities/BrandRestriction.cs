 using E_Commerce.Domain.SharedKernel.Abstract;

namespace E_Commerce.Domain.ContextBounded.Catalog.AggregateRoots.Brand.Entities
{
    public class BrandRestriction : BaseEntity
    {
        public RestrictionType Type { get; private set; }
        public string Name { get; private set; } = null!;   
        public string Description { get; private set; } = null!;
        public string RestrictionValue { get; private set; } = null!;// e.g., country code, channel ID, price threshold
        public DateTime? EffectiveFrom { get; private set; }
        public DateTime? EffectiveTo { get; private set; }
        public RestrictionStatus Status { get; private set; }
        public int Priority { get; private set; } // lower number = higher priority
        public Guid BrandId { get; private set; }

        public static BrandRestriction Create(
            RestrictionType type,
            string name,
            string description,
            string restrictionValue,
            Guid brandId,
            DateTime? effectiveFrom = null,
            DateTime? effectiveTo = null,
            int priority = 0)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidOperationException("Restriction name is required.");
            if (string.IsNullOrWhiteSpace(restrictionValue))
                throw new InvalidOperationException("Restriction value is required.");
            if (effectiveFrom.HasValue && effectiveTo.HasValue && effectiveFrom >= effectiveTo)
                throw new InvalidOperationException("EffectiveFrom must be before EffectiveTo.");

            var restriction = new BrandRestriction
            {
                Id = Guid.NewGuid(),
                Type = type,
                Name = name.Trim(),
                Description = description.Trim(),
                RestrictionValue = restrictionValue.Trim(),
                EffectiveFrom = effectiveFrom,
                EffectiveTo = effectiveTo,
                Status = RestrictionStatus.Active,
                Priority = priority,
                BrandId = brandId,
            };

            return restriction;
        }

        public void UpdateDetails(
            string name,
            string description,
            string restrictionValue,
            DateTime? effectiveFrom,
            DateTime? effectiveTo,
            int priority,
            string updatedBy)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidOperationException("Restriction name is required.");
            if (string.IsNullOrWhiteSpace(restrictionValue))
                throw new InvalidOperationException("Restriction value is required.");
            if (effectiveFrom.HasValue && effectiveTo.HasValue && effectiveFrom >= effectiveTo)
                throw new InvalidOperationException("EffectiveFrom must be before EffectiveTo.");

            Name = name.Trim();
            Description = description.Trim();
            RestrictionValue = restrictionValue.Trim();
            EffectiveFrom = effectiveFrom;
            EffectiveTo = effectiveTo;
            Priority = priority;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Activate(string activatedBy)
        {
            if (Status == RestrictionStatus.Active)
                return;
            Status = RestrictionStatus.Active;
            UpdatedAt = DateTime.UtcNow;            
        }

        public void Deactivate(string reason, string deactivatedBy)
        {
            if (Status == RestrictionStatus.Inactive)
                return;
            Status = RestrictionStatus.Inactive;
            UpdatedAt = DateTime.UtcNow;           
        }

        public void Expire()
        {
            if (Status == RestrictionStatus.Expired)
                return;
            Status = RestrictionStatus.Expired;
            UpdatedAt = DateTime.UtcNow;
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

    public enum RestrictionType
    {
        Geographic,
        Channel,
        MinimumAdvertisedPrice,
        Inventory,
        Promotional,
        Display
    }

    public enum RestrictionStatus
    {
        Active,
        Inactive,
        Expired
    }
}