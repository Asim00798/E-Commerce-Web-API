using E_Commerce.Domain.BoundedContexts.UserManagement.Profiles;
using E_Commerce.Domain.BoundedContexts.CoreCommerce.Reviews___Engagement;
using E_Commerce.Domain.SharedKernel.Abstract;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Reviews
{
    public class Wishlist : BaseEntity
    {
        public Guid CustomerProfileId { get; set; }
        public CustomerProfile CustomerProfile { get; set; } = null!;

        public string Name { get; set; } = "Default Wishlist";

        public ICollection<WishlistItem>? Items { get; set; }

        public override void Validate()
        {
            base.Validate();

            if (CustomerProfileId == Guid.Empty)
                throw new InvalidOperationException("Wishlist must belong to a CustomerProfile.");

            if (string.IsNullOrWhiteSpace(Name))
                throw new InvalidOperationException("Wishlist Name cannot be empty.");
        }
    }
}
