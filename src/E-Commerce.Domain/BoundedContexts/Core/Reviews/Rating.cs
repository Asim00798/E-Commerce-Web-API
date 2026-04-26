#if false
using E_Commerce.Domain.BoundedContexts.CoreCommerce.Catalog.AggregateRoot.Product;
using E_Commerce.Domain.BoundedContexts.UserManagement.Profiles;
using E_Commerce.Domain.SharedKernel.Abstract;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Reviews
{
    public class Rating : BaseEntity
    {
        public Guid CustomerProfileId { get; set; }
        public CustomerProfile CustomerProfile { get; set; } = null!;

        public Guid ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public int Value { get; set; } // 1 to 5
        public string? Comment { get; set; }

        public override void Validate()
        {
            base.Validate();

            if (CustomerProfileId == Guid.Empty)
                throw new InvalidOperationException("Rating must be linked to a CustomerProfile.");

            if (ProductId == Guid.Empty)
                throw new InvalidOperationException("Rating must be linked to a Product.");

            if (Value < 1 || Value > 5)
                throw new InvalidOperationException("Rating Value must be between 1 and 5.");
        }
    }
}

#endif