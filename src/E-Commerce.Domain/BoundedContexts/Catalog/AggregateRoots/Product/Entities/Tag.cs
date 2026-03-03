using E_Commerce.Domain.BoundedContexts.Catalog.ValueObjects;
using E_Commerce.Domain.SharedKernel.Abstract;

namespace E_Commerce.Domain.BoundedContexts.Catalog.AggregateRoots.Product.Entities
{
    public class Tag : BaseEntity
    {
        public TagName Name { get; private set; }

        // DDD constructor
        public Tag(string name)
        {
            Name = new TagName(name);
        }

    }
}
