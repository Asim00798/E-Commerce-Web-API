using E_Commerce.Domain.ContextBounded.Catalog.AggregateRoots.Brand.Entities;

namespace E_Commerce.Domain.BoundedContexts.Catalog.AggregateRoots.Brand.Behaviors
{
    public partial class Brand
    {
        private readonly List<BrandRestriction> _restrictions = new();
        public IReadOnlyCollection<BrandRestriction> Restrictions => _restrictions.AsReadOnly();

        public BrandRestriction AddRestriction(
            RestrictionType type,
            string name,
            string description,
            string restrictionValue,
            string createdBy,
            DateTime? effectiveFrom = null,
            DateTime? effectiveTo = null,
            int priority = 0)
        {
            // Brand-level invariant: maybe limit number of active restrictions
            if (_restrictions.Count(r => r.IsEffectiveAt(DateTime.UtcNow)) >= 20)
                throw new InvalidOperationException("Brand cannot have more than 20 active restrictions.");

            var restriction = BrandRestriction.Create(
                type, name, description, restrictionValue, Id, createdBy,
                effectiveFrom, effectiveTo, priority);

            _restrictions.Add(restriction);
            return restriction;
        }

        public void RemoveRestriction(Guid restrictionId, string removedBy)
        {
            var restriction = _restrictions.FirstOrDefault(r => r.Id == restrictionId);
            if (restriction == null)
                throw new InvalidOperationException("Restriction not found.");

            // Soft delete
            restriction.Deactivate("Removed by user", removedBy);
            _restrictions.Remove(restriction); // or keep for history
        }

        public IEnumerable<BrandRestriction> GetActiveRestrictions(DateTime? atDate = null)
        {
            var date = atDate ?? DateTime.UtcNow;
            return _restrictions.Where(r => r.IsEffectiveAt(date));
        }

        public bool HasRestriction(RestrictionType type, string value, DateTime? atDate = null)
        {
            var date = atDate ?? DateTime.UtcNow;
            return _restrictions.Any(r => r.Type == type &&
                                           r.RestrictionValue.Equals(value, StringComparison.OrdinalIgnoreCase) &&
                                           r.IsEffectiveAt(date));
        }
    }
}
