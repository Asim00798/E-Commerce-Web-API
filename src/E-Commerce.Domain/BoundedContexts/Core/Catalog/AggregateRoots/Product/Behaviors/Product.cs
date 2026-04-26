using E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Product.Entities;
using E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Product.Events;
using E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Product.ValueObjects;
using E_Commerce.Domain.BoundedContexts.Core.Catalog.Enums;
using E_Commerce.Domain.BoundedContexts.Core.Compliance.Evaluation;
using E_Commerce.Domain.SharedKernel.Abstractions;
using E_Commerce.Domain.SharedKernel.ValueObjects;

namespace E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Product.Behaviors
{
    public partial class Product : BaseEntity,IAggregateRoot, IComplianceTarget
    {
        private readonly List<ProductImage> _images = new();
        private readonly List<ProductVariant> _variants = new();
        private readonly List<ProductAttribute> _attributes = new();
        private readonly List<Tag> _tags = new();

        public ProductDescription Description { get; private set; }
        public ProductStatus Status { get; private set; }

        public Guid CategoryId { get; private set; }
        public Guid? BrandId { get; private set; }

        public IReadOnlyCollection<ProductImage> Images => _images;
        public IReadOnlyCollection<ProductVariant> Variants => _variants;
        public IReadOnlyCollection<ProductAttribute> Attributes => _attributes;
        public IReadOnlyCollection<Tag> Tags => _tags;

        public Product(ProductDescription description, Guid categoryId, Guid? brandId = null)
        {
            Description = description;
            CategoryId = categoryId;
            BrandId = brandId;
            Status = ProductStatus.Draft;

            AddDomainEvent(new ProductDrafted(Id));
        }

        /// <summary>
        /// Pure static factory to encapsulate the creation of a Product aggregate.
        /// Following Senior DDD patterns: logic stays here, but events are emitted by the aggregate itself.
        /// </summary>
        public static Product Create(
            string description,
            Guid categoryId,
            Guid? brandId = null)
        {
            if (string.IsNullOrWhiteSpace(description)) throw new ArgumentNullException(nameof(description));

            var productDescription = new ProductDescription(description);

            return new Product(
                productDescription,
                categoryId,
                brandId
            );
        }
    }
}
