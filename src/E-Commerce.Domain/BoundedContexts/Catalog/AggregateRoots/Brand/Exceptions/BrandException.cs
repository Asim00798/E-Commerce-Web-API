using E_Commerce.Domain.BoundedContexts.Catalog.Exceptions;

namespace E_Commerce.Domain.BoundedContexts.Catalog.AggregateRoots.Brand.Exceptions
{
    /// <summary>
    /// Base exception for all exceptions related to the Brand aggregate root.
    /// </summary>
    public abstract class BrandException : CatalogException
    {
        protected BrandException() { }

        protected BrandException(string message)
            : base(message) { }

        protected BrandException(string message, Exception inner)
            : base(message, inner) { }
    }
}
