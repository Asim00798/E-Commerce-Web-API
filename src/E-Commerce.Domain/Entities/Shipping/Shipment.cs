using E_Commerce.Domain.Entities.Abstract;
using E_Commerce.Domain.Entities.Ordering;


namespace E_Commerce.Domain.Entities.Shipping
{
    public class Shipment : BaseEntity
    {
        public Guid OrderId { get; set; }
        public Guid DeliveryMethodId { get; set; }

        public DateTimeOffset ShippedAt { get; set; }
        public DateTimeOffset? DeliveredAt { get; set; }
        public string? TrackingNumber { get; set; }

        // Navigation
        public Order? Order { get; set; }
        public DeliveryMethod? DeliveryMethod { get; set; }
        public ICollection<ShipmentItem>? Items { get; set; }

        public override void Validate()
        {
            base.Validate();

            if (OrderId == Guid.Empty)
                throw new InvalidOperationException("Shipment must be linked to an Order.");
        }
    }
}
