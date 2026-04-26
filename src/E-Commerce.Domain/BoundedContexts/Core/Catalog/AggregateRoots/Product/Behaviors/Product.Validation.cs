using E_Commerce.Domain.BoundedContexts.Core.Catalog.Enums;
using E_Commerce.Domain.SharedKernel.Exceptions;

namespace E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Product.Behaviors
{
    public partial class Product
    {
        private void EnsureIsDraft()
        {
            if (Status != ProductStatus.Draft)
                throw new BusinessRuleViolationException("Only draft products can be modified.");
        }

    }
}
