#if false
using E_Commerce.Domain.SharedKernel.Rules;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Recommendation.Rules
{
    public abstract class RecommendationBusinessRule : IBusinessRule
    {
        public abstract bool IsSatisfied();
        public abstract string Message { get; }
    }
}

#endif