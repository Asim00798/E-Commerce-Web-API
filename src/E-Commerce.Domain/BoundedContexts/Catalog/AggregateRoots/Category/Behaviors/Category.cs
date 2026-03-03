using E_Commerce.Domain.BoundedContexts.Catalog.AggregateRoots.Category.Entities;
using E_Commerce.Domain.BoundedContexts.Catalog.ValueObjects;
using E_Commerce.Domain.SharedKernel.Abstract;
using E_Commerce.Domain.SharedKernel.Interfaces;

namespace E_Commerce.Domain.BoundedContexts.Catalog.AggregateRoots.Category.Behaviors
{
    public partial class Category : BaseEntity,IAggregateRoot
    {
        private readonly List<CategoryHierarchy> _hierarchies = new();
        public IReadOnlyCollection<CategoryHierarchy> Hierarchies => _hierarchies;

        private readonly List<CategoryImage> _images = new();
        public IReadOnlyCollection<CategoryImage> Images => _images;

        private readonly List<CategoryAttribute> _attributes = new();
        public IReadOnlyCollection<CategoryAttribute> Attributes => _attributes;

        private readonly List<Guid> _productIds = new();
        public IReadOnlyCollection<Guid> ProductIds => _productIds;

        public Category(/*CategoryInfo info*/)
        {
            
        }

    }

}
