using E_Commerce.Domain.BoundedContexts.CoreCommerce.Catalog.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Catalog.AggregateRoots.Category.Exceptions
{
    /// <summary>
    /// Base exception for all exceptions related to the Category aggregate root.
    /// </summary>
    public abstract class CategoryException : CatalogException
    {
        protected CategoryException() { }

        protected CategoryException(string message)
            : base(message) { }

        protected CategoryException(string message, Exception inner)
            : base(message, inner) { }
    }
}
