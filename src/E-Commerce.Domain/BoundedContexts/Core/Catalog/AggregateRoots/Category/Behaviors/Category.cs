using E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Category.Entities;
using E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Category.Events;
using E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Category.Exceptions;
using E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Category.ValueObjects;
using E_Commerce.Domain.SharedKernel.Abstractions;

namespace E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Category.Behaviors
{
    public partial class Category : BaseEntity, IAggregateRoot
    {
        private readonly List<CategoryHierarchy> _hierarchies = new();
        private readonly List<CategoryImage> _images = new();
        private readonly List<CategoryAttribute> _attributes = new();
        private readonly List<Guid> _productIds = new();

        public CategoryInfo Info { get; private set; }
        public Guid OwnerId { get; private set; }

        public IReadOnlyCollection<CategoryHierarchy> Hierarchies => _hierarchies.AsReadOnly();
        public IReadOnlyCollection<CategoryImage> Images => _images.AsReadOnly();
        public IReadOnlyCollection<CategoryAttribute> Attributes => _attributes.AsReadOnly();
        public IReadOnlyCollection<Guid> ProductIds => _productIds.AsReadOnly();

        public Category(CategoryInfo info, Guid ownerId)
        {
            Info = info ?? throw new ArgumentNullException(nameof(info));
            OwnerId = ownerId;

            AddDomainEvent(new CategoryCreatedEvent(Id, Info.Name.Value));
        }

        public static Category Create(string name, Guid ownerId, string? description = null)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new CategoryException("Category name is required.");
            var categoryName = new CategoryName(name);
            var categoryInfo = new CategoryInfo(categoryName, description);
            return new Category(categoryInfo, ownerId);
        }
    }
}
