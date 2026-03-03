using E_Commerce.Domain.BoundedContexts.Catalog.AggregateRoots.Product.Exceptions;
using E_Commerce.Domain.ContextBounded.Catalog.AggregateRoots.Brand.Entities;
using E_Commerce.Domain.ContextBounded.Catalog.AggregateRoots.Product.Entities;

namespace E_Commerce.Domain.BoundedContexts.Catalog.AggregateRoots.Product.Behaviors
{
    public partial class Product
    {
        private readonly List<ProductRestriction> _restrictions = new();
        public IReadOnlyCollection<ProductRestriction> Restrictions => _restrictions.AsReadOnly();

        public ProductRestriction AddRestriction(
            RestrictionType type,
            string restrictionValue,
            string createdBy,
            DateTime? effectiveFrom = null,
            DateTime? effectiveTo = null,
            int priority = 0)
        {
            // Product-level invariants: maybe limit number of active restrictions
            if (_restrictions.Count(r => r.IsEffectiveAt(DateTime.UtcNow)) >= 10)
                throw new ProductException("Product cannot have more than 10 active restrictions.");

            var restriction = ProductRestriction.Create(
                type, restrictionValue, Id, createdBy, effectiveFrom, effectiveTo, priority);

            _restrictions.Add(restriction);
            AddDomainEvent(new ProductRestrictionAdded(Id, restriction.Id, createdBy));
            return restriction;
        }

        public void RemoveRestriction(Guid restrictionId, string removedBy)
        {
            var restriction = _restrictions.FirstOrDefault(r => r.Id == restrictionId);
            if (restriction == null)
                throw new ProductException("Restriction not found.");

            // Soft delete – deactivate and remove from active collection
            restriction.Deactivate("Removed by user", removedBy);
            _restrictions.Remove(restriction); // or keep for history
            AddDomainEvent(new ProductRestrictionRemoved(Id, restrictionId, removedBy));
        }

        public IEnumerable<ProductRestriction> GetActiveRestrictions(DateTime? atDate = null)
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
