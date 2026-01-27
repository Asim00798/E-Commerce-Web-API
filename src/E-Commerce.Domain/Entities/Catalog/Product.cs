using E_Commerce.Domain.Entities.Abstract;
using E_Commerce.Domain.Entities.Catalog;
using E_Commerce.Domain.Entities.Ordering;
using E_Commerce.Domain.Entities.Reviews;

namespace E_Commerce.Domain.Entities.Catalog
{
    public class Product : BaseEntity
    {
        public string Name { get; set; } = string.Empty;      // Required
        public string? Description { get; set; }
        public decimal Price { get; set; }                     // Required, non-nullable

        public Guid CategoryId { get; set; }                  // FK to category
        public Category? Category { get; set; }

        public Guid? BrandId { get; set; }                    // Optional FK
        public Brand? Brand { get; set; }

        // Navigation
        public ICollection<ProductImage> Images { get; set; } = new HashSet<ProductImage>();
        public ICollection<ProductVariant> Variants { get; set; } = new HashSet<ProductVariant>();
        public ICollection<ProductAttribute> Attributes { get; set; } = new HashSet<ProductAttribute>();
        public ICollection<Tag> Tags { get; set; } = new HashSet<Tag>();

        // Optional for cross-context reference
        public ICollection<OrderItem>? OrderItems { get; set; }
        public ICollection<Review>? Reviews { get; set; }

        // Validation to enforce domain invariants
        public override void Validate()
        {
            base.Validate();

            if (string.IsNullOrWhiteSpace(Name))
                throw new InvalidOperationException("Product name cannot be empty.");

            if (Price < 0)
                throw new InvalidOperationException("Product price cannot be negative.");

        }
    }
}
