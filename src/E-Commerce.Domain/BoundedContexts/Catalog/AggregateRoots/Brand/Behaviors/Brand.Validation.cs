using E_Commerce.Domain.SharedKernel.Exceptions;

namespace E_Commerce.Domain.BoundedContexts.Catalog.AggregateRoots.Brand.Behaviors
{
    public partial class Brand
    {
        public override void Validate()
        {
            base.Validate();

            if (_logos.Count == 0)
                throw new BusinessRuleViolationException("Brand must have at least one logo.");
        }
    }
}
