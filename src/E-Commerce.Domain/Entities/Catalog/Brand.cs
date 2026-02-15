using E_Commerce.Domain.Entities.Abstract;
using E_Commerce.Domain.Exceptions;
using E_Commerce.Domain.ValueObjects;

namespace E_Commerce.Domain.Entities.Catalog
{
    public class Brand : BaseEntity
    {
        public BrandDescription Description { get; private set; } = null!;

        // Navigation
        private readonly List<Product> _products = new();
        public IReadOnlyCollection<Product> Products => _products;

        // DDD-style constructor enforcing invariants
        public Brand(BrandDescription description)
        {
            if (description == null)
                throw new NotAllowedOperationException("Brand creation", "Brand description cannot be empty.");

            Description = description;
        }

        public void ChangeDescription(BrandDescription description)
        {
            if (description == null)
                throw new BusinessRuleViolationException("Brand description cannot be empty.");

            if (Description == description) return;

            Description = description;
        }

    }

}
