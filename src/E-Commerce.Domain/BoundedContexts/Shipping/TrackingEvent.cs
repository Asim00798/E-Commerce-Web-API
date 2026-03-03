using E_Commerce.Domain.SharedKernel.Abstract;

namespace E_Commerce.Domain.BoundedContexts.Shipping
{
    public class TrackingEvent : BaseEntity
    {
        public Guid ShipmentId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? CurrentLocation { get; set; }
        public DateTimeOffset OccurredAt { get; set; } = DateTimeOffset.UtcNow;

        // Navigation
        public Shipment? Shipment { get; set; }

        public override void Validate()
        {
            base.Validate();

            if (ShipmentId == Guid.Empty)
                throw new InvalidOperationException("TrackingEvent must belong to a Shipment.");

            if (string.IsNullOrWhiteSpace(Status))
                throw new InvalidOperationException("TrackingEvent Status is required.");
        }
    }
}
