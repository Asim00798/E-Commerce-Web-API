using E_Commerce.Domain.SharedKernel.Exceptions;

namespace E_Commerce.Domain.BoundedContexts.Core.Ordering.AggregateRoots.Cart.Exceptions;

public sealed class CartException : DomainException
{
    public CartException(string message) : base(message)
    {}

    public CartException(string message, Exception innerException) : base(message, innerException)
    {}
}