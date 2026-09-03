namespace E_Commerce.Domain.BoundedContexts.Core.Ordering.AggregateRoots.Order.Enums;

public enum OrderStatus
{
    PendingPayment = 1,
    PaymentFailed = 2,
    Paid = 3,
    Shipped = 4,
    Delivered = 5,
    Cancelled = 6,
    Returned = 7,
    Refunded = 8
}