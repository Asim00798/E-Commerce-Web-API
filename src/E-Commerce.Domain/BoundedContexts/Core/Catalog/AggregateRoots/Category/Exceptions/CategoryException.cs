using E_Commerce.Domain.BoundedContexts.Core.Catalog.Exceptions;

namespace E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Category.Exceptions
{
    /// <summary>
    /// Base exception for all exceptions related to the Category aggregate root.
    /// </summary>
    public class CategoryException : CatalogException
    {
        protected CategoryException() { }

        public CategoryException(string message)
            : base(message) { }

        public CategoryException(string message, Exception inner)
            : base(message, inner) { }
    }
}
