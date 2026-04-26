#if false
using E_Commerce.Domain.SharedKernel.Rules;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Recommendation.AggregateRoots.UserPreference.Rules
{
    public class InteractionMustBeValidRule : IBusinessRule
    {
        private readonly Guid _productId;

        public InteractionMustBeValidRule(Guid productId)
        {
            _productId = productId;
        }

        public bool IsSatisfied() => _productId != Guid.Empty;

        public string Message => "Interaction must be associated with a valid product.";
    }
}

#endif