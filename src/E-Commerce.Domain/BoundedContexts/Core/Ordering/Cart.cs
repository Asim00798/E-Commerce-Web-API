using E_Commerce.Domain.BoundedContexts.CoreCommerce.Catalog;
using E_Commerce.Domain.BoundedContexts.UserManagement.Identity;
using E_Commerce.Domain.Exceptions;
using E_Commerce.Domain.Events.Ordering.Cart;
using E_Commerce.Domain.SharedKernel.Abstract;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Ordering
{
    public class Cart : BaseEntity
    {
        public Guid UserId { get; private set; } // Owner of the cart

        // Navigation
        public User? User { get; private set; }
        public ICollection<CartItem> Items { get; private set; } = new HashSet<CartItem>();

        public decimal TotalAmount => Items?.Sum(i => i.TotalPrice) ?? 0;

        // DDD Constructor
        public Cart(Guid userId)
        {
            if (userId == Guid.Empty)
                throw new BusinessRuleViolationException("Cart must have a valid UserId.");

            UserId = userId;
            AddDomainEvent(new CartCreated(Id));
        }

        public void AddItem(Guid productId, int quantity, decimal unitPrice)
        {
            var existingItem = Items.FirstOrDefault(x => x.ProductId == productId);
            if (existingItem != null)
            {
                existingItem.UpdateQuantity(existingItem.Quantity + quantity);
            }
            else
            {
                Items.Add(new CartItem(Id, productId, quantity, unitPrice));
            }
            // per refined strategy, item additions don't emit domain events unless it's a fact like "OrderPlaced"
        }

        public void RemoveItem(Guid productId)
        {
            var item = Items.FirstOrDefault(x => x.ProductId == productId);
            if (item != null)
            {
                Items.Remove(item);
            }
        }

        public void Clear()
        {
            if (Items.Count == 0) return;

            Items.Clear();
            AddDomainEvent(new CartCleared(Id));
        }

        public void Expire()
        {
            AddDomainEvent(new CartExpired(Id));
        }

        public override void Validate()
        {
            base.Validate();

            if (UserId == Guid.Empty)
                throw new InvalidOperationException("Cart must have a valid UserId.");
        }
    }
}
