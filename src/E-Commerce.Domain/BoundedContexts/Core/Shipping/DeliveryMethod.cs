using E_Commerce.Domain.SharedKernel.Abstract;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Shipping
{
    public class DeliveryMethod : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Cost { get; set; }
        public int EstimatedDays { get; set; } // Estimated delivery time in days
        public bool IsActive { get; set; } = true;

        public override void Validate()
        {
            base.Validate();

            if (string.IsNullOrWhiteSpace(Name))
                throw new InvalidOperationException("DeliveryMethod Name is required.");

            if (Cost < 0)
                throw new InvalidOperationException("DeliveryMethod Cost cannot be negative.");

            if (EstimatedDays <= 0)
                throw new InvalidOperationException("EstimatedDays must be greater than zero.");
        }
    }
}
