using E_Commerce.Domain.SharedKernel.Exceptions;

namespace E_Commerce.Domain.BoundedContexts.Core.Ordering.AggregateRoots.Order.Exceptions;

public sealed class OrderException : DomainException
{
    public OrderException(string message) : base(message)
    {}

    public OrderException(string message, Exception innerException) : base(message, innerException)
    {}
}