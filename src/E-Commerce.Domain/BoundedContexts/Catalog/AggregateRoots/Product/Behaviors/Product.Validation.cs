using E_Commerce.Domain.BoundedContexts.Catalog.Enums;
using E_Commerce.Domain.SharedKernel.Exceptions;

namespace E_Commerce.Domain.BoundedContexts.Catalog.AggregateRoots.Product.Behaviors
{
    public partial class Product
    {
        private void EnsureIsDraft()
        {
            if (Status != ProductStatus.Draft)
                throw new BusinessRuleViolationException("Only draft products can be modified.");
        }
        public override void Validate()
        {
            base.Validate();

            if (Price.Amount < 0)
                throw new BusinessRuleViolationException("Product price cannot be negative.");
        }
    }
}
