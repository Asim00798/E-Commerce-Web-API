using E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Product.Events;
using E_Commerce.Domain.BoundedContexts.Core.Catalog.Enums;
using E_Commerce.Domain.SharedKernel.Exceptions;

namespace E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Product.Behaviors
{
    public partial class Product
    {
        public void Publish()
        {
            EnsureIsDraft();

            if (!_variants.Any())
                throw new BusinessRuleViolationException("Product must have variants.");

            Status = ProductStatus.Published;
            AddDomainEvent(new ProductPublished(Id));
        }

        public void Discontinue()
        {
            if (Status == ProductStatus.Discontinued) return;

            Status = ProductStatus.Discontinued;
            AddDomainEvent(new ProductDiscontinued(Id));
        }
 
    }
}
