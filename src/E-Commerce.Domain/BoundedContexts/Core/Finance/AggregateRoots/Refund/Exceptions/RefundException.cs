using E_Commerce.Domain.SharedKernel.Exceptions;

namespace E_Commerce.Domain.BoundedContexts.Core.Finance.AggregateRoots.Refund.Exceptions;

public sealed class RefundException : DomainException
{
    public RefundException(string message)
        : base(message)
    {
    }

    public RefundException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}