#if false
using E_Commerce.Domain.SharedKernel.Rules;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Promotion.Rules
{
    public abstract class PromotionBusinessRule : IBusinessRule
    {
        public abstract bool IsSatisfied();
        public abstract string Message { get; }
    }
}

#endif