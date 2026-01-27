using E_Commerce.Domain.Entities.Abstract;
using E_Commerce.Domain.Entities.Catalog;

namespace E_Commerce.Domain.Entities.Shipping
{
    public class ShipmentItem : BaseEntity
    {
        public Guid ShipmentId { get; set; }
        public Guid ProductId { get; set; }
        public Guid? ProductVariantId { get; set; }
        public int Quantity { get; set; }

        // Navigation
        public Shipment? Shipment { get; set; }
        public Product? Product { get; set; }
        public ProductVariant? ProductVariant { get; set; }

        public override void Validate()
        {
            base.Validate();

            if (ShipmentId == Guid.Empty)
                throw new InvalidOperationException("ShipmentItem must belong to a Shipment.");

            if (Quantity <= 0)
                throw new InvalidOperationException("ShipmentItem quantity must be greater than zero.");
        }
    }
}
