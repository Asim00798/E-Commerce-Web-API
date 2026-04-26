using E_Commerce.Domain.SharedKernel.Exceptions;

namespace E_Commerce.Domain.BoundedContexts.Core.Catalog.Exceptions
{
    /// <summary>
    /// CatalogException serves as the base class for all exceptions related to
    /// catalog operations within the catalog context
    /// </summary>
    public abstract class CatalogException : DomainException
    {
        protected CatalogException() { }

        protected CatalogException(string message)
            : base(message) { }

        protected CatalogException(string message, Exception inner)
            : base(message, inner) { }
    }
}

