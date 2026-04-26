using E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Brand.ValueObjects;

namespace E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Brand.Behaviors
{
    public partial class Brand
    {
        public void UpdateDescription(BrandDescription newDescription)
        {
            Description = newDescription ?? throw new ArgumentNullException(nameof(newDescription));
        }
    }
}
