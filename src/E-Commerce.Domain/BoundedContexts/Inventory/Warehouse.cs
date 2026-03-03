using E_Commerce.Domain.SharedKernel.Abstract;
using E_Commerce.Domain.SharedKernel.ValueObjects;

namespace E_Commerce.Domain.BoundedContexts.Inventory
{
    public class Warehouse : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public Address? Address { get; set; }
        public string? Code { get; set; } // Optional warehouse code
        public bool IsActive { get; set; } = true;

        // Navigation
        public ICollection<Inventory>? Inventories { get; set; }

        public override void Validate()
        {
            base.Validate();

            if (string.IsNullOrWhiteSpace(Name))
                throw new InvalidOperationException("Warehouse name cannot be empty.");
        }
    }
}
