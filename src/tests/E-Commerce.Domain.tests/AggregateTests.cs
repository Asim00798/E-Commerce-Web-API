using E_Commerce.Domain.Entities.Catalog;
using E_Commerce.Domain.Entities.Ordering;
using E_Commerce.Domain.DomainEvents.Catalog.Product;
using E_Commerce.Domain.DomainEvents.Ordering.Order;
using Xunit;
using E_Commerce.Domain.SharedKernel.Exceptions;
using E_Commerce.Domain.SharedKernel.Enums;

namespace E_Commerce.Domain.Tests
{
    public class AggregateTests
    {
        [Fact]
        public void Product_Creation_ShouldRaiseProductDrafted()
        {
            // Arrange
            var categoryId = Guid.NewGuid();

            // Act
            var product = new Product("Test Product", "Description", 100m, categoryId);

            // Assert
            Assert.Equal(ProductStatus.Draft, product.Status);
            Assert.Contains(product.DomainEvents, e => e is ProductDrafted);
        }

        [Fact]
        public void Product_Publish_ShouldRaiseProductPublished()
        {
            // Arrange
            var product = new Product("Test Product", "Description", 100m, Guid.NewGuid());
            product.ClearDomainEvents();

            // Act
            product.Publish();

            // Assert
            Assert.Equal(ProductStatus.Published, product.Status);
            Assert.Contains(product.DomainEvents, e => e is ProductPublished);
        }

        [Fact]
        public void Product_Publish_WhenNotDraft_ShouldThrow()
        {
            // Arrange
            var product = new Product("Test Product", "Description", 100m, Guid.NewGuid());
            product.Publish();

            // Act & Assert
            Assert.Throws<BusinessRuleViolationException>(() => product.Publish());
        }

        [Fact]
        public void Order_Place_ShouldRaiseOrderPlaced()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var addressId = Guid.NewGuid();

            // Act
            var order = new Order(userId, addressId, "ORD-123");

            // Assert
            Assert.Equal(OrderStatus.Pending, order.Status);
            Assert.Contains(order.DomainEvents, e => e is OrderPlaced);
        }

        [Fact]
        public void Order_Pay_ShouldRaiseOrderPaid()
        {
            // Arrange
            var order = new Order(Guid.NewGuid(), Guid.NewGuid(), "ORD-123");
            order.ClearDomainEvents();

            // Act
            order.Pay(Guid.NewGuid());

            // Assert
            Assert.Equal(OrderStatus.Paid, order.Status);
            Assert.Contains(order.DomainEvents, e => e is OrderPaid);
        }
    }
}
