using E_Commerce.Domain.Entities.Abstract;
using E_Commerce.Domain.Entities.Profiles;
using E_Commerce.Domain.Entities.Reviews___Engagement;

namespace E_Commerce.Domain.Entities.Reviews
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
