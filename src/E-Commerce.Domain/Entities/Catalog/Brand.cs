using E_Commerce.Domain.Entities.Abstract;
using E_Commerce.Domain.Exceptions;
using E_Commerce.Domain.ValueObjects;

namespace E_Commerce.Domain.Entities.Catalog
{
    public class Brand : BaseEntity
    {
        public BrandDescription Description { get; private set; } = null!;
        public ICollection<Product>? Products { get; set; }

        public Brand(BrandDescription description)
        {
            if (description == null)
                throw new NotAllowedOperationException("Brand creation", "Brand description cannot be empty.");

            Description = description;
        }
    }

}
