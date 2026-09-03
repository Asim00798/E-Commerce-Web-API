using E_Commerce.Domain.BoundedContexts.Core.Ordering.AggregateRoots.Order.Enums;
using E_Commerce.Domain.BoundedContexts.Core.Ordering.AggregateRoots.Order.Events;
using E_Commerce.Domain.BoundedContexts.Core.Ordering.AggregateRoots.Order.Exceptions;

namespace E_Commerce.Domain.BoundedContexts.Core.Ordering.AggregateRoots.Order.Behaviors;

public sealed partial class Order
{
    public void MarkPaymentFailed()
    {
        if (Status != OrderStatus.PendingPayment)
            throw new OrderException("Only pending payment orders can be marked as payment failed.");

        Status = OrderStatus.PaymentFailed;
    }

    public void MarkPaid()
    {
        if (Status != OrderStatus.PendingPayment)
            throw new OrderException("Only pending payment orders can be marked as paid.");

        Status = OrderStatus.Paid;

        AddDomainEvent(new OrderPaidDomainEvent(Id, CustomerId));
    }

    public void MarkShipped()
    {
        if (Status != OrderStatus.Paid)
            throw new OrderException("Only paid orders can be marked as shipped.");

        Status = OrderStatus.Shipped;
    }

    public void MarkDelivered()
    {
        if (Status != OrderStatus.Shipped)
            throw new OrderException("Only shipped orders can be marked as delivered.");

        Status = OrderStatus.Delivered;
        DeliveredAtUtc = DateTime.UtcNow;
    }

    public void MarkReturned()
    {
        if (Status != OrderStatus.Delivered && Status != OrderStatus.Shipped)
            throw new OrderException("Only shipped or delivered orders can be returned.");

        Status = OrderStatus.Returned;
    }

    public void MarkRefunded()
    {
        if (Status != OrderStatus.Paid &&
            Status != OrderStatus.Delivered &&
            Status != OrderStatus.Returned)
        {
            throw new OrderException(
                "Only paid, delivered, or returned orders can be refunded.");
        }

        Status = OrderStatus.Refunded;
        RefundedAtUtc = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (Status != OrderStatus.PendingPayment && Status != OrderStatus.PaymentFailed)
            throw new OrderException("Only pending payment or payment failed orders can be cancelled.");

        Status = OrderStatus.Cancelled;
        CancelledAtUtc = DateTime.UtcNow;

        AddDomainEvent(new OrderCancelledDomainEvent(Id, CustomerId));
    }
}