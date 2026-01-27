using E_Commerce.Domain.Entities.Abstract;
using E_Commerce.Domain.Entities.Catalog;
using E_Commerce.Domain.Entities.Finance;
using E_Commerce.Domain.Entities.Identity;

namespace E_Commerce.Domain.Entities.Ordering
{
    public class Order : BaseEntity
    {
        public string OrderNumber { get; set; } = string.Empty;
        public Guid UserId { get; set; }
        public Guid ShippingAddressId { get; set; }
        public Guid? PaymentId { get; set; }

        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public decimal TotalAmount { get; set; }
        public decimal ShippingFee { get; set; } = 0;
        public decimal TaxAmount { get; set; } = 0;

        public DateTimeOffset PlacedAt { get; set; } = DateTimeOffset.UtcNow;

        // Navigation
        public User? User { get; set; }
        public ShippingAddress? ShippingAddress { get; set; }
        public Payment? Payment { get; set; }
        public ICollection<OrderItem> Items { get; set; } = new HashSet<OrderItem>();
        public ICollection<OrderStatusHistory>? StatusHistory { get; set; }

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
        Shipped = 2,
        Delivered = 3,
        Cancelled = 4,
        Refunded = 5
    }
}
