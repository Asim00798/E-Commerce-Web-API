using E_Commerce.Domain.DomainEvents;
using System;

namespace E_Commerce.Application.ApplicationEvents.Reviews.WishlistItem
{
    public sealed class WishlistItemNotifiedBackInStock : DomainEvent
    {
        public Guid WishlistItemNotifiedBackInStockId { get; }

        public WishlistItemNotifiedBackInStock(Guid wishlistItemNotifiedBackInStockId)
        {
            WishlistItemNotifiedBackInStockId = wishlistItemNotifiedBackInStockId;
        }
    }
}