using E_Commerce.Domain.Entities.Abstract;
using E_Commerce.Domain.ValueObjects;

namespace E_Commerce.Domain.Entities.Catalog
{
    public class Tag : BaseEntity
    {
        public TagName Name { get; private set; } 

        // DDD constructor
        public Tag(string name)
        {
            Name = new TagName(name);
        }
        // Navigation
        public ICollection<Product>? Products { get; set; }
    }
}
