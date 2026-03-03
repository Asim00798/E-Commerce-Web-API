using E_Commerce.Domain.BoundedContexts.Shipping;
using E_Commerce.Domain.BoundedContexts.Ordering;
using E_Commerce.Domain.SharedKernel.Abstract;

namespace E_Commerce.Domain.BoundedContexts.Shipping
{
    public class Delivery : BaseEntity
    {
        public Guid OrderId { get; set; }
        public Order? Order { get; set; }

        public Guid? DeliveryMethodId { get; set; }
        public DeliveryMethod? DeliveryMethod { get; set; }

        public string? Description { get; set; }
        public DateTimeOffset? DeliveryTime { get; set; }
        public decimal? Cost { get; set; }
        public DeliveryStatus Status { get; set; } = DeliveryStatus.Pending;

        public override void Validate()
        {
            base.Validate();

            if (OrderId == Guid.Empty)
                throw new InvalidOperationException("Delivery must be linked to an Order.");

            if (Cost < 0)
                throw new InvalidOperationException("Delivery cost cannot be negative.");
        }
    }

    public enum DeliveryStatus
    {
        Pending,
        Shipped,
        Delivered,
        Canceled
    }
}
