#if false
using E_Commerce.Domain.SharedKernel.Exceptions;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Pricing.Exceptions
{
    public abstract class PricingException : DomainException
    {
        protected PricingException() { }

        protected PricingException(string message)
            : base(message) { }

        protected PricingException(string message, Exception inner)
            : base(message, inner) { }
    }
}

#endif