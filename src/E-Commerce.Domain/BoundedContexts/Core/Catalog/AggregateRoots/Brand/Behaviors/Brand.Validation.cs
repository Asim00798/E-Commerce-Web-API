using E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Brand.Exceptions;

namespace E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Brand.Behaviors
{
    public partial class Brand
    {
        public override void Validate()
        {
            base.Validate();

            if (_logos.Count == 0)
                throw new BrandException("Brand must have at least one logo.");
        }
    }
}
