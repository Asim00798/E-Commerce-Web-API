#if false
using E_Commerce.Domain.SharedKernel.Exceptions;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.MarketPlace.Exceptions
{
    public abstract class MarketplaceException : DomainException
    {
        protected MarketplaceException() { }

        protected MarketplaceException(string message)
            : base(message) { }

        protected MarketplaceException(string message, Exception inner)
            : base(message, inner) { }
    }
}

#endif