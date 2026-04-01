using E_Commerce.Domain.BoundedContexts.CoreCommerce.Catalog.AggregateRoots.Product.Entities;
using E_Commerce.Domain.BoundedContexts.CoreCommerce.Catalog.AggregateRoots.Product.ValueObjects;
using E_Commerce.Domain.BoundedContexts.CoreCommerce.Catalog.Enums;
using E_Commerce.Domain.SharedKernel.Abstract;
using E_Commerce.Domain.SharedKernel.Interfaces;
using E_Commerce.Domain.SharedKernel.ValueObjects;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Catalog.AggregateRoots.Product.Behaviors
{
    public partial class Product : BaseEntity,IAggregateRoot
    {
        private readonly List<ProductImage> _images = new();
        private readonly List<ProductVariant> _variants = new();
        private readonly List<ProductAttribute> _attributes = new();
        private readonly List<Tag> _tags = new();

        public ProductDescription Description { get; private set; }
        public Money Price { get; private set; }
        public ProductStatus Status { get; private set; }

        public Guid CategoryId { get; private set; }
        public Guid? BrandId { get; private set; }

        public IReadOnlyCollection<ProductImage> Images => _images;
        public IReadOnlyCollection<ProductVariant> Variants => _variants;
        public IReadOnlyCollection<ProductAttribute> Attributes => _attributes;
        public IReadOnlyCollection<Tag> Tags => _tags;

        public Product(ProductDescription description, Money price, Guid categoryId, Guid? brandId = null)
        {
            Description = description;
            Price = price;
            CategoryId = categoryId;
            BrandId = brandId;
            Status = ProductStatus.Draft;

            AddDomainEvent(new ProductDrafted(Id));
        }
    }
}
