using E_Commerce.Domain.BoundedContexts.Core.Ordering.AggregateRoots.Order.Entities;
using E_Commerce.Domain.BoundedContexts.Core.Ordering.AggregateRoots.Order.Enums;
using E_Commerce.Domain.BoundedContexts.Core.Ordering.AggregateRoots.Order.Events;
using E_Commerce.Domain.BoundedContexts.Core.Ordering.AggregateRoots.Order.Exceptions;
using E_Commerce.Domain.SharedKernel.Abstractions;
using E_Commerce.Domain.SharedKernel.ValueObjects;
using System.Runtime.CompilerServices;

namespace E_Commerce.Domain.BoundedContexts.Core.Ordering.AggregateRoots.Order.Behaviors;

public sealed partial class Order : BaseEntity, IAggregateRoot
{
    private readonly List<OrderItem> _items = new();

    public Guid CustomerId { get; private set; }
    public OrderStatus Status { get; private set; }
    public Money Subtotal { get; private set; } = null!;
    public Money ShippingFee { get; private set; } = null!;
    public Money Total { get; private set; } = null!;

    public DateTime PlacedAtUtc { get; private set; }
    public DateTime? CancelledAtUtc { get; private set; }
    public DateTime? DeliveredAtUtc { get; private set; }
    public DateTime? RefundedAtUtc { get; private set; }

    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    private Order()
    {
        // EF Core
    }

    private Order(
    Guid customerId,
    IEnumerable<OrderItem> items,
    Money shippingFee)
    {
        CustomerId = customerId;
        _items.AddRange(items);
        Subtotal = CalculateSubtotal(items);
        ShippingFee = shippingFee;
        Total = Subtotal.Add(ShippingFee);
        Status = OrderStatus.PendingPayment;
        PlacedAtUtc = DateTime.UtcNow;

        // Set the order ID on each item now that the order's ID exists.
        foreach (var item in _items)
        {
            item.SetOrderId(Id);
        }

        AddDomainEvent(new OrderPlacedDomainEvent(Id, CustomerId, Total));
    }

    public static Order Place(
        Guid customerId,
        IEnumerable<OrderItem> items,
        Money shippingFee)
    {
        if (customerId == Guid.Empty)
            throw new OrderException("Customer ID is required.");

        var orderItems = items?.ToList() ?? new List<OrderItem>();

        if (orderItems.Count == 0)
            throw new OrderException("Order must have at least one item.");

        if (shippingFee is null || shippingFee.Amount < 0)
            throw new OrderException("Shipping fee cannot be negative.");

        EnsureSameCurrency(orderItems, shippingFee);

        return new Order(customerId, orderItems, shippingFee);
    }

    private static Money CalculateSubtotal(IEnumerable<OrderItem> items)
    {
        var firstCurrency = items.First().UnitPrice.Currency;
        var totalAmount = items.Sum(x => x.LineTotal.Amount);
        return new Money(totalAmount, firstCurrency);
    }

    private static void EnsureSameCurrency(IEnumerable<OrderItem> items, Money shippingFee)
    {
        var currencies = items.Select(x => x.UnitPrice.Currency).Distinct().ToList();

        if (currencies.Count > 1)
            throw new OrderException("All order items must have the same currency.");

        if (shippingFee.Currency != currencies.First())
            throw new OrderException("Shipping fee currency must match order items currency.");
    }

    public OrderItem Create(      
        Guid productId,
        Guid productVariantId,
        string sku,
        string productName,
        string variantName,
        Money unitPrice,
        int quantity)
    {        

        return new OrderItem(
            productId,
            productVariantId,
            sku,
            productName,
            variantName,
            unitPrice,
            quantity);
    }
}