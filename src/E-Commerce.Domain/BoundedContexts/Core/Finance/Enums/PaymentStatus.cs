namespace E_Commerce.Domain.BoundedContexts.Core.Finance.Enums;

public enum PaymentStatus
{
    Pending = 1,
    AwaitingPayment = 2,
    Captured = 3,
    Failed = 4,
    Cancelled = 5,
    PartiallyRefunded = 6,
    Refunded = 7
}