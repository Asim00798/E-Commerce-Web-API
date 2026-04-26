#if false
using E_Commerce.Domain.SharedKernel.Exceptions;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Promotion.Exceptions
{
    public abstract class PromotionException : DomainException
    {
        protected PromotionException() { }

        protected PromotionException(string message)
            : base(message) { }

        protected PromotionException(string message, Exception inner)
            : base(message, inner) { }
    }
}

#endif