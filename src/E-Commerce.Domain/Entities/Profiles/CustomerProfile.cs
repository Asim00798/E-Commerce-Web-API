using E_Commerce.Domain.Entities.Abstract;
using E_Commerce.Domain.Entities.Identity;
using E_Commerce.Domain.Entities.Ordering;
using E_Commerce.Domain.Entities.PersonalData;
using E_Commerce.Domain.Entities.Reviews;
using E_Commerce.Domain.Entities.Reviews___Engagement;

namespace E_Commerce.Domain.Entities.Profiles
{
    public class CustomerProfile : BaseEntity
    {
        public Guid UserId { get; set; } // Link to User
        public User User { get; set; } = null!;

        public Guid? PersonId { get; set; }
        public Person? Person { get; set; }

        // Orders
        public ICollection<Order> Orders { get; set; } = new HashSet<Order>();

        // Shipping addresses
        public ICollection<ShippingAddress> ShippingAddresses { get; set; } = new HashSet<ShippingAddress>();

        // Wishlist & Reviews
        //public ICollection<Wishlist>? Wishlists { get; set; } // To be implemented later
        public ICollection<Review>? Reviews { get; set; }

        // Optional loyalty / points
        public int LoyaltyPoints { get; set; } = 0;

        public override void Validate()
        {
            base.Validate();

            if (UserId == Guid.Empty)
                throw new InvalidOperationException("CustomerProfile must have a valid UserId.");
        }
    }
}
