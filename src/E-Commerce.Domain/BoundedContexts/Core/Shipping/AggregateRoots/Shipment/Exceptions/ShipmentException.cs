using E_Commerce.Domain.SharedKernel.Exceptions;

namespace E_Commerce.Domain.BoundedContexts.Core.Shipping.AggregateRoots.Shipment.Exceptions;

public sealed class ShipmentException : DomainException
{
    public ShipmentException(string message)
        : base(message)
    {
    }

    public ShipmentException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}