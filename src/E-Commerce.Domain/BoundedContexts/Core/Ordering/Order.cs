using Domain.Orders.Events;
using E_Commerce.Domain.BoundedContexts.Core.Ordering.ValueObjects;
using E_Commerce.Domain.BoundedContexts.CoreCommerce.Ordering.Ordering.Order;
using E_Commerce.Domain.SharedKernel.Abstractions;
using E_Commerce.Domain.SharedKernel.Exceptions;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Ordering
{
    public class Order : BaseEntity
    {
        public string OrderNumber { get; private set; } = string.Empty;
        public Guid UserId { get; private set; }
        public Guid CustomerId { get; private set; }
        public string ShippingAddress { get; private set; } = string.Empty;
        public Guid? PaymentId { get; private set; }
        public List <OrderLine> Lines { get; private set; } = new List<OrderLine>();
        public OrderStatus Status { get; private set; } = OrderStatus.Pending;
        public decimal TotalAmount { get; private set; }
        public decimal ShippingFee { get; private set; } = 0;
        public decimal TaxAmount { get; private set; } = 0;
        public DateTimeOffset PlacedAt { get; private set; } = DateTimeOffset.UtcNow;

        //// Navigation
        //public User? User { get; private set; }
        //public Address? ShippingAddress { get; private set; }
        //public Payment? Payment { get; private set; }
        //public ICollection<OrderItem> Items { get; private set; } = new HashSet<OrderItem>();
        //public ICollection<OrderStatusHistory>? StatusHistory { get; private set; }

        // DDD Constructor
        public Order(Guid userId, string shippingAddress, string orderNumber)
        {
            if (string.IsNullOrWhiteSpace(orderNumber))
                throw new BusinessRuleViolationException("Order number cannot be empty.");

            UserId = userId;
            ShippingAddress = shippingAddress;
            OrderNumber = orderNumber;
            Status = OrderStatus.Pending;
            PlacedAt = DateTimeOffset.UtcNow;

            AddDomainEvent(new OrderPlacedDomainEvent(Id, CustomerId, TotalAmount));
        }

        /// <summary>
        /// Updates the order's shipping address. No domain event is raised – this is a
        /// pure state change that does not require any side‑effects.
        /// </summary>
        public void ChangeShippingAddress(string newAddress)
        {
            if (string.IsNullOrWhiteSpace(newAddress))
                throw new ArgumentException("Shipping address cannot be empty.", nameof(newAddress));

            ShippingAddress = newAddress;
        }

        //public void ApplyDiscount(Discount discount)
        //{
        //    //if (Status != OrderStatus.Draft)
        //    //    throw new DomainException("Cannot discount finalized order.");

        //    //Total = Total.Subtract(discount.Amount);
        //    //AddDomainEvent(new OrderDiscountApplied(Id, discount.Id));
        //}
        //public void Pay(Guid paymentId)
        //{
        //    if (Status != OrderStatus.Pending)
        //        throw new BusinessRuleViolationException("Only pending orders can be paid.");

        //    PaymentId = paymentId;
        //    Status = OrderStatus.Paid;
        //    AddDomainEvent(new OrderPaid(Id));
        //}

        //public void Confirm()
        //{
        //    if (Status != OrderStatus.Paid)
        //        throw new BusinessRuleViolationException("Only paid orders can be confirmed.");

        //    Status = OrderStatus.Confirmed;
        //    AddDomainEvent(new OrderConfirmed(Id));
        //}

        //public void Ship()
        //{
        //    if (Status != OrderStatus.Confirmed)
        //        throw new BusinessRuleViolationException("Only confirmed orders can be shipped.");

        //    Status = OrderStatus.Shipped;
        //    AddDomainEvent(new OrderShipped(Id));
        //}

        //public void Deliver()
        //{
        //    if (Status != OrderStatus.Shipped)
        //        throw new BusinessRuleViolationException("Only shipped orders can be delivered.");

        //    Status = OrderStatus.Delivered;
        //    AddDomainEvent(new OrderDelivered(Id));
        //}

        //public void Cancel(string reason)
        //{
        //    if (Status == OrderStatus.Shipped || Status == OrderStatus.Delivered)
        //        throw new BusinessRuleViolationException("Cannot cancel an order that has already been shipped or delivered.");

        //    Status = OrderStatus.Cancelled;
        //    AddDomainEvent(new OrderCancelled(Id));
        //}

        //public void Refund()
        //{
        //    if (Status != OrderStatus.Paid && Status != OrderStatus.Delivered)
        //        throw new BusinessRuleViolationException("Only paid or delivered orders can be refunded.");

        //    Status = OrderStatus.Refunded;
        //    AddDomainEvent(new OrderRefunded(Id));
        //}

        //public void AddItem(OrderItem item)
        //{
        //    if (Status != OrderStatus.Pending)
        //        throw new BusinessRuleViolationException("Cannot add items to an order that is not pending.");

        //    Items.Add(item);
        //    RecalculateTotal();
        //}

        //private void RecalculateTotal()
        //{
        //    TotalAmount = Items.Sum(x => x.UnitPrice * x.Quantity) + ShippingFee + TaxAmount;
        //}
        public static Order Place(Guid customerId, List<OrderLine> lines)
        {
            if (lines == null || !lines.Any())
                throw new BusinessRuleViolationException("Order must have at least one line item.");
            var order = new Order(customerId, "Default Shipping Address", Guid.NewGuid().ToString());
            order.Lines.AddRange(lines);
            order.TotalAmount = lines.Sum(line => line.Quantity * line.Price);
            order.AddDomainEvent(new OrderPlacedDomainEvent(order.Id, customerId, order.TotalAmount));
            return order;
        }
        /// <summary>
        /// Marks the order as delivered and records the corresponding domain event.
        /// </summary>
        public void MarkAsDelivered()
        {
            if (Status != OrderStatus.Shipped)
                throw new BusinessRuleViolationException("Only shipped orders can be marked as delivered.");

            Status = OrderStatus.Delivered;
            AddDomainEvent(new OrderDelivered(Id));
        }
        public void Expire()
        {
            if (Status != OrderStatus.Pending)
                throw new BusinessRuleViolationException("Only pending orders can be expired.");
            Status = OrderStatus.Expired;
        }
        public override void Validate()
        {
            base.Validate();

            if (string.IsNullOrWhiteSpace(OrderNumber))
                throw new InvalidOperationException("OrderNumber cannot be empty.");

            if (TotalAmount < 0)
                throw new InvalidOperationException("TotalAmount cannot be negative.");
        }
    }

    public enum OrderStatus
    {
        Pending = 0,
        Paid = 1,
        Confirmed = 2,
        Shipped = 3,
        Delivered = 4,
        Cancelled = 5,
        Expired = 7,
        Refunded = 6
    }
}

