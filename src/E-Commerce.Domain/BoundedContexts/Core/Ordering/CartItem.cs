#if false
using E_Commerce.Domain.BoundedContexts.CoreCommerce.Catalog.AggregateRoot.Product;
using E_Commerce.Domain.BoundedContexts.CoreCommerce.Catalog.AggregateRoots.Product.Entities;
using E_Commerce.Domain.Exceptions;
using E_Commerce.Domain.SharedKernel.Abstract;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Ordering
{
    public class CartItem : BaseEntity
    {
        public Guid CartId { get; private set; }
        public Guid ProductId { get; private set; }
        public Guid? ProductVariantId { get; private set; }
        public int Quantity { get; private set; } = 1;
        public decimal UnitPrice { get; private set; }

        // Navigation
        public Cart? Cart { get; private set; }
        public Product? Product { get; private set; }
        public ProductVariant? ProductVariant { get; private set; }

        public decimal TotalPrice => UnitPrice * Quantity;

        // DDD Constructor
        public CartItem(Guid cartId, Guid productId, int quantity, decimal unitPrice, Guid? productVariantId = null)
        {
            if (quantity <= 0)
                throw new BusinessRuleViolationException("Quantity must be greater than zero.");

            CartId = cartId;
            ProductId = productId;
            Quantity = quantity;
            UnitPrice = unitPrice;
            ProductVariantId = productVariantId;
        }

        public void UpdateQuantity(int quantity)
        {
            if (quantity <= 0)
                throw new BusinessRuleViolationException("Quantity must be greater than zero.");

            Quantity = quantity;
        }

        public override void Validate()
        {
            base.Validate();

            if (Quantity <= 0)
                throw new InvalidOperationException("Quantity must be greater than zero.");
        }
    }
}

#endif