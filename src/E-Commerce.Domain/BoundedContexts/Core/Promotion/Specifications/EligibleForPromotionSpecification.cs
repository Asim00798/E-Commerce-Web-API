#if false
using System.Linq.Expressions;
using E_Commerce.Domain.SharedKernel.Specifications;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Promotion.Specifications
{
    public class EligibleForPromotionSpecification<T> : ISpecification<T>
    {
        public Expression<Func<T, bool>> ToExpression()
        {
            return x => true; // Generic eligibility logic
        }

        public bool IsSatisfiedBy(T entity)
        {
            return true;
        }
    }
}

#endif