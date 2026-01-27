using E_Commerce.Domain.Entities.Abstract;
using E_Commerce.Domain.Entities.Catalog;
using E_Commerce.Domain.Entities.Identity;

namespace E_Commerce.Domain.Entities.Ordering
{
    public class Cart : BaseEntity
    {
        public Guid UserId { get; set; } // Owner of the cart

        // Navigation
        public User? User { get; set; }
        public ICollection<CartItem> Items { get; set; } = new HashSet<CartItem>();

        public decimal TotalAmount => Items?.Sum(i => i.TotalPrice) ?? 0;

        public override void Validate()
        {
            base.Validate();

            if (UserId == Guid.Empty)
                throw new InvalidOperationException("Cart must have a valid UserId.");
        }
    }
}
