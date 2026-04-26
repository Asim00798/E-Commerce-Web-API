#if false
using E_Commerce.Domain.SharedKernel.Rules;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.MarketPlace.Rules
{
    public abstract class MarketplaceBusinessRule : IBusinessRule
    {
        public abstract bool IsSatisfied();
        public abstract string Message { get; }
    }
}

#endif