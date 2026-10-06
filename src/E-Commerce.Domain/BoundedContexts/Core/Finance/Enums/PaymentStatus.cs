namespace E_Commerce.Domain.BoundedContexts.Core.Finance.Enums;

public enum PaymentStatus
{
    Pending = 1,
    AwaitingPayment = 2,
    Captured = 3,
    Failed = 4,
    PartiallyRefunded = 5,
    Refunded = 6
}