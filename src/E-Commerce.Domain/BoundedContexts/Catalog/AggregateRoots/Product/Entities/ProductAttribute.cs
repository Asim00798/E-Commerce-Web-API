using E_Commerce.Domain.BoundedContexts.Catalog.AggregateRoots.Product.Features;
using E_Commerce.Domain.SharedKernel.Abstract;

namespace E_Commerce.Domain.BoundedContexts.Catalog.AggregateRoots.Product.Entities
{
    public class ProductAttribute: BaseEntity
    {
        public Guid ProductId { get; private set; }
        public string Name { get; private set; } = string.Empty;
        public string Value { get; private set; } = string.Empty;

        public ProductAttribute(Guid productId, string name, string value)
        {
            ProductId = productId;
            Name = name;
            Value = value;
        }

        public void UpdateValue(string value)
        {
            Value = value;
        }
    }
}
