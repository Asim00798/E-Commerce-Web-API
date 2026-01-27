using E_Commerce.Domain.Entities.Abstract;

namespace E_Commerce.Domain.Entities.Finance
{
    public class Discount : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Amount { get; set; }
        public bool IsPercentage { get; set; } = false;
        public bool IsActive { get; set; } = true;

        // Optional target
        public Guid? ProductId { get; set; }
        public Guid? CategoryId { get; set; }

        public override void Validate()
        {
            base.Validate();

            if (string.IsNullOrWhiteSpace(Name))
                throw new InvalidOperationException("Discount name cannot be empty.");

            if (Amount <= 0)
                throw new InvalidOperationException("Discount amount must be greater than zero.");
        }
    }
}
