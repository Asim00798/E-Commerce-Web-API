
using E_Commerce.Domain.BoundedContexts.Catalog.Exceptions;
using E_Commerce.Domain.SharedKernel.Exceptions;

namespace E_Commerce.Domain.BoundedContexts.Catalog.AggregateRoots.Product.Exceptions
{

    /// <summary>
    /// ProductException serves as the base class for all exceptions related to
    /// product operations within the catalog context.
    /// It inherits from CatalogException, 
    /// allowing it to be caught as a more general catalog-related exception 
    /// when necessary.
    /// This design promotes a clear and organized exception hierarchy,
    /// making it easier to handle specific product-related errors
    /// while still maintaining the ability to catch broader catalog exceptions
    /// when needed.
    /// </summary>
    public abstract class ProductException : CatalogException
    {
        protected ProductException() { }

        protected ProductException(string message)
            : base(message) { }

        protected ProductException(string message, Exception inner)
            : base(message, inner) { }
    }
}
