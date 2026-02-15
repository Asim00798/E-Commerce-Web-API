using E_Commerce.Domain.Entities.Abstract;
using E_Commerce.Domain.Entities.Ordering;
using E_Commerce.Domain.Entities.Reviews;
using E_Commerce.Domain.Enums;
using E_Commerce.Domain.Exceptions;
using E_Commerce.Domain.DomainEvents.Catalog.Product;
using E_Commerce.Domain.ValueObjects;

namespace E_Commerce.Domain.Entities.Catalog
{
    public class Product : BaseEntity
    {
        public ProductDescription ProductDescription { get; set; }
        public Money Price { get; private set; }                  // Required, non-nullable
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
        public Product(ProductDescription productDescription, Money price, Guid categoryId, Guid? brandId = null)
        {
            ProductDescription = productDescription;
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

        public void SetMainImage(ProductImage image)
        {
            foreach (var img in Images)
                img.IsMain = false;

            image.IsMain = true;
        }

        public void AddVariant(ProductVariant variant)
        {
            if (Variants.Any(v => v.Name == variant.Name || v.SKU == variant.SKU))
                throw new BusinessRuleViolationException("Duplicate variant name or SKU not allowed.");

            Variants.Add(variant);
        }

        public void AdjustVariantStock(Guid variantId, int delta)
        {
            var variant = Variants.FirstOrDefault(v => v.Id == variantId)
                ?? throw new BusinessRuleViolationException("Variant not found.");

            if (variant.StockQuantity + delta < 0)
                throw new BusinessRuleViolationException("Stock cannot be negative.");

            variant.StockQuantity += delta;
        }

        public override void Validate()
        {
            base.Validate();

            if (Price.Amount < 0)
                throw new BusinessRuleViolationException("Product price cannot be negative.");
        }
    }
}
