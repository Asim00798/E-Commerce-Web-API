
namespace E_Commerce.Domain.BoundedContexts.Core.Ordering.ValueObjects
{
    public class OrderLine
    {
        public Guid ProductId { get; private set; }
        public int Quantity { get; private set; }
        public decimal Price { get; private set; }

        public OrderLine(Guid productId, int quantity, decimal price)
        {
            ProductId = productId;
            Quantity = quantity;
            Price = price;
        }
    }
}
