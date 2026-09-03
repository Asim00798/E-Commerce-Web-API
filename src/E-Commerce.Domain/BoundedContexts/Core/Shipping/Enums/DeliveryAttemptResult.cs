namespace E_Commerce.Domain.BoundedContexts.Core.Shipping.Enums;

public enum DeliveryAttemptResult
{
    Delivered = 1,
    CustomerUnavailable = 2,
    WrongAddress = 3,
    CustomerRefused = 4,
    Other = 5
}