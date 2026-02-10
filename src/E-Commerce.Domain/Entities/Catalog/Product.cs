using E_Commerce.Domain.Entities.Abstract;
using E_Commerce.Domain.Entities.Catalog;
using E_Commerce.Domain.Entities.Ordering;
using E_Commerce.Domain.Entities.Reviews;
using E_Commerce.Domain.Enums;
using E_Commerce.Domain.Exceptions;
using E_Commerce.Domain.DomainEvents.Catalog.Product;

namespace E_Commerce.Domain.Entities.Catalog
{
    public class Product : BaseEntity
    {
        public string Name { get; private set; } = string.Empty;      // Required
        public string? Description { get; private set; }
        public decimal Price { get; private set; }                     // Required, non-nullable
        public ProductStatus Status { get; private set; } = ProductStatus.Draft;

        public Guid CategoryId { get; private set; }                  // FK to category
        public Category? Category { get; private set; }

        public Guid? BrandId { get; private set; }                    // Optional FK
        public Brand? Brand { get; private set; }

        // Navigation
        public ICollection<ProductImage> Images { get; private set; } = new HashSet<ProductImage>();
        public ICollection<ProductVariant> Variants { get; private set; } = new HashSet<ProductVariant>();
        public ICollection<ProductAttribute> Attributes { get; private set; } = new HashSet<ProductAttribute>();
        public ICollection<Tag> Tags { get; private set; } = new HashSet<Tag>();

        // Optional for cross-context reference
        public ICollection<OrderItem>? OrderItems { get; private set; }
        public ICollection<Review>? Reviews { get; private set; }

        // DDD Constructor
        public Product(string name, string? description, decimal price, Guid categoryId, Guid? brandId = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new BusinessRuleViolationException("Product name cannot be empty.");
            
            if (price < 0)
                throw new BusinessRuleViolationException("Product price cannot be negative.");

            Name = name;
            Description = description;
            Price = price;
            CategoryId = categoryId;
            BrandId = brandId;
            Status = ProductStatus.Draft;

            AddDomainEvent(new ProductDrafted(Id));
        }

        // Methods for behavior
        public void Publish()
        {
            if (Status != ProductStatus.Draft)
                throw new BusinessRuleViolationException("Product must be in Draft state to be published.");

            Status = ProductStatus.Published;
            AddDomainEvent(new ProductPublished(Id));
        }

        public void Discontinue()
        {
            if (Status == ProductStatus.Discontinued) return;

            Status = ProductStatus.Discontinued;
            AddDomainEvent(new ProductDiscontinued(Id));
        }

        public void AdjustPrice(decimal newPrice)
        {
            if (newPrice < 0)
                throw new BusinessRuleViolationException("Product price cannot be negative.");

            if (Price == newPrice) return;

            Price = newPrice;
            AddDomainEvent(new ProductPriceAdjusted(Id));
        }

        public void UpdateDescription(string? description)
        {
            // Business rule: description changes are allowed but don't emit events per refined strategy
            Description = description;
        }

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
