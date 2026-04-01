using E_Commerce.Domain.BoundedContexts.CoreCommerce.Catalog.AggregateRoots.Product.ValueObjects;
using E_Commerce.Domain.SharedKernel.Abstract;
using E_Commerce.Domain.SharedKernel.Interfaces;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Catalog.AggregateRoots.Product.Entities
{
    public class Tag : BaseEntity, IEntity<Tag>
    {
        public TagName Name { get; private set; }

        // DDD constructor
        public Tag(string name)
        {
            Name = new TagName(name);
        }

    }
}
