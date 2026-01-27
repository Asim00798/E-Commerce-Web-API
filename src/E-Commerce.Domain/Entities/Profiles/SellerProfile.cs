using E_Commerce.Domain.Entities.Abstract;
using E_Commerce.Domain.Entities.Catalog;
using E_Commerce.Domain.Entities.Identity;
using E_Commerce.Domain.Entities.PersonalData;

namespace E_Commerce.Domain.Entities.Profiles
{
    public class SellerProfile : BaseEntity
    {
        public Guid UserId { get; set; } // Link to User
        public User User { get; set; } = null!;

        public Guid? PersonId { get; set; }
        public Person? Person { get; set; }

        // Shop information
        public string ShopName { get; set; } = string.Empty;
        public string? ShopDescription { get; set; }
        public string? ShopLogoUrl { get; set; }
        public string? ShopBannerUrl { get; set; }
        public Address? ShopAddress { get; set; }
        public List<Contact>? ShopContacts { get; set; }

        // Products
        public ICollection<Product>? Products { get; set; }

        // Ratings & reviews
        public ICollection<Review>? Reviews { get; set; }

        public bool IsVerified { get; set; } = false;

        public override void Validate()
        {
            base.Validate();

            if (UserId == Guid.Empty)
                throw new InvalidOperationException("SellerProfile must have a valid UserId.");

            if (string.IsNullOrWhiteSpace(ShopName))
                throw new InvalidOperationException("ShopName is required for SellerProfile.");
        }
    }
}
