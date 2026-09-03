using E_Commerce.Domain.SharedKernel.Exceptions;

namespace E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Category.Exceptions;

public sealed class CategoryException : DomainException
{
    public CategoryException(string message) : base(message)
    {
    }

    public CategoryException(string message, Exception innerException) : base(message, innerException)
    {
    }
}