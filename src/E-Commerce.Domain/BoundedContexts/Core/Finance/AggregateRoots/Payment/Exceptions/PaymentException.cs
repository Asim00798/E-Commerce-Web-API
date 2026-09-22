using E_Commerce.Domain.SharedKernel.Exceptions;

namespace E_Commerce.Domain.BoundedContexts.Core.Finance.AggregateRoots.Payment.Exceptions;

public sealed class PaymentException : DomainException
{
    public PaymentException(string message)
        : base(message)
    {
    }

    public PaymentException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}