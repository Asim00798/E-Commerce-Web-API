#if false
using E_Commerce.Domain.BoundedContexts.CoreCommerce.Catalog.AggregateRoot.Product;
using E_Commerce.Domain.BoundedContexts.UserManagement.Profiles;
using E_Commerce.Domain.SharedKernel.Abstract;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Reviews
{
    public class Review : BaseEntity
    {
        public Guid CustomerProfileId { get; set; }
        public CustomerProfile CustomerProfile { get; set; } = null!;

        public Guid ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;

        public int RatingValue { get; set; } // Can be 1-5 stars
        public bool IsApproved { get; set; } = false;

        public override void Validate()
        {
            base.Validate();

            if (CustomerProfileId == Guid.Empty)
                throw new InvalidOperationException("Review must be linked to a CustomerProfile.");

            if (ProductId == Guid.Empty)
                throw new InvalidOperationException("Review must be linked to a Product.");

            if (string.IsNullOrWhiteSpace(Content))
                throw new InvalidOperationException("Review content cannot be empty.");

            if (RatingValue < 1 || RatingValue > 5)
                throw new InvalidOperationException("RatingValue must be between 1 and 5.");
        }
    }
}

#endif