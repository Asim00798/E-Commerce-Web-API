using E_Commerce.Domain.Entities.Abstract;

namespace E_Commerce.Domain.Entities.Finance
{
    public class Tax : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public decimal Rate { get; set; } // e.g., 0.05 = 5%
        public bool IsActive { get; set; } = true;

        // Optional targets
        public Guid? ProductId { get; set; }
        public Guid? CategoryId { get; set; }

        public override void Validate()
        {
            base.Validate();

            if (string.IsNullOrWhiteSpace(Name))
                throw new InvalidOperationException("Tax name cannot be empty.");

            if (Rate < 0 || Rate > 1)
                throw new InvalidOperationException("Tax rate must be between 0 and 1.");
        }
    }
}
