using E_Commerce.Domain.Entities.Abstract;
using E_Commerce.Domain.Entities.Catalog;

namespace E_Commerce.Domain.Entities.Reviews
{
    public class WishlistItem : BaseEntity
    {
        public Guid WishlistId { get; set; }
        public Guid ProductId { get; set; }

        public Wishlist Wishlist { get; set; } = null!;
        public Product Product { get; set; } = null!;

        public int Quantity { get; set; } = 1;

        public override void Validate()
        {
            base.Validate();

            if (WishlistId == Guid.Empty)
                throw new InvalidOperationException("WishlistItem must belong to a Wishlist.");

            if (ProductId == Guid.Empty)
                throw new InvalidOperationException("WishlistItem must be linked to a Product.");

            if (Quantity <= 0)
                throw new InvalidOperationException("WishlistItem quantity must be greater than zero.");
        }
    }
}
