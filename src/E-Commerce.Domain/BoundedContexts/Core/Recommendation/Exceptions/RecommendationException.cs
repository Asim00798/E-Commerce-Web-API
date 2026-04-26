#if false
using E_Commerce.Domain.SharedKernel.Exceptions;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Recommendation.Exceptions
{
    public abstract class RecommendationException : DomainException
    {
        protected RecommendationException() { }

        protected RecommendationException(string message)
            : base(message) { }

        protected RecommendationException(string message, Exception inner)
            : base(message, inner) { }
    }
}

#endif