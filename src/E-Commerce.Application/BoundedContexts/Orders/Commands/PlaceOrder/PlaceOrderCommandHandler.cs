using E_Commerce.Application.Shared.Models;
using E_Commerce.Application.Shared.Observability.Metrics;
using E_Commerce.Application.Shared.Security.Identity;
using E_Commerce.Application.Shared.Shipping.Models;
using E_Commerce.Application.Shared.Shipping.Services;
using E_Commerce.Application.Shared.Stock;
using E_Commerce.Domain.BoundedContexts.Core.Ordering.AggregateRoots.Cart.Behaviors;
using E_Commerce.Domain.BoundedContexts.Core.Ordering.AggregateRoots.Order.Behaviors;
using E_Commerce.Domain.BoundedContexts.Core.Ordering.AggregateRoots.Order.Entities;
using E_Commerce.Domain.BoundedContexts.Core.Ordering.Repositories;
using E_Commerce.Domain.BoundedContexts.UserManagement.People.AggregateRoots.Person.Behaviors;
using E_Commerce.Domain.BoundedContexts.UserManagement.Registration.Repositories;
using E_Commerce.Domain.SharedKernel.Exceptions;
using E_Commerce.Domain.SharedKernel.PersistenceAbstractions;
using E_Commerce.Domain.SharedKernel.ValueObjects;
using MediatR;

namespace E_Commerce.Application.BoundedContexts.Orders.Commands.PlaceOrder;

public sealed class PlaceOrderCommandHandler
    : IRequestHandler<PlaceOrderCommand, Result<Guid>>
{
    private readonly ICartRepository _cartRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IPersonRepository _personRepository;
    private readonly IStockService _stockService;
    private readonly IShippingFeeCalculator _shippingFeeCalculator;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _unitOfWork;
    private readonly OrderMetrics _metrics;

    public PlaceOrderCommandHandler(
        ICartRepository cartRepository,
        IOrderRepository orderRepository,
        IPersonRepository personRepository,
        IStockService stockService,
        IShippingFeeCalculator shippingFeeCalculator,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork,
        OrderMetrics metrics)
    {
        _cartRepository = cartRepository;
        _orderRepository = orderRepository;
        _personRepository = personRepository;
        _stockService = stockService;
        _shippingFeeCalculator = shippingFeeCalculator;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
        _metrics = metrics;
    }

    public async Task<Result<Guid>> Handle(
        PlaceOrderCommand request,
        CancellationToken ct)
    {
        var customerId = GetCustomerId();
        if (customerId is null)
            return Result<Guid>.Failure("Customer not found.");

        return await ExecuteOrderPlacementAsync(customerId.Value, ct);
    }

    #region Private Methods
    private Guid? GetCustomerId()
    {
        return _currentUser.UserId;
    }

    private async Task<Result<Guid>> ExecuteOrderPlacementAsync(
        Guid customerId,
        CancellationToken ct)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync(ct);

            var preparationResult = await PrepareOrderAsync(customerId, ct);
            if (!preparationResult.Succeeded)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return Result<Guid>.Failure(preparationResult.Errors);
            }

            var order = preparationResult.Data!.Order;
            var cart = preparationResult.Data.Cart;

            await PersistOrderAsync(order, cart, ct);
            await _unitOfWork.SaveChangesAsync(ct);
            await _unitOfWork.CommitTransactionAsync(ct);

            RecordOrderCreatedMetric();

            return Result<Guid>.Success(order.Id);
        }
        catch (DomainException ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            return Result<Guid>.Failure(ex.Message);
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    private async Task<Result<OrderPreparationResult>> PrepareOrderAsync(
        Guid customerId,
        CancellationToken ct)
    {
        var cartResult = await LoadCartAsync(customerId, ct);
        if (!cartResult.Succeeded)
            return Result<OrderPreparationResult>.Failure(cartResult.Errors);
        var cart = cartResult.Data!;

        var personResult = await LoadPersonAsync(customerId, ct);
        if (!personResult.Succeeded)
            return Result<OrderPreparationResult>.Failure(personResult.Errors);
        var person = personResult.Data!;

        var shippingFeeResult = await CalculateShippingFeeAsync(person, ct);
        if (!shippingFeeResult.Succeeded)
            return Result<OrderPreparationResult>.Failure(shippingFeeResult.Errors);
        var shippingFee = shippingFeeResult.Data!;

        var orderItems = BuildOrderItems(cart);

        var stockResult = await DecreaseStockAsync(orderItems, ct);
        if (!stockResult.Succeeded)
            return Result<OrderPreparationResult>.Failure(stockResult.Errors);

        var order = CreateOrder(customerId, orderItems, shippingFee);

        return Result<OrderPreparationResult>.Success(
            new OrderPreparationResult(order, cart));
    }

    private async Task<Result<Cart>> LoadCartAsync(
        Guid customerId,
        CancellationToken ct)
    {
        var cart = await _cartRepository.GetByCustomerIdAsync(customerId, ct);
        if (cart is null || cart.Items.Count == 0)
            return Result<Cart>.Failure("Cart is empty or not found.");

        return Result<Cart>.Success(cart);
    }

    private async Task<Result<Person>> LoadPersonAsync(
        Guid customerId,
        CancellationToken ct)
    {
        var person = await _personRepository.GetByIdentityUserIdAsync(customerId, ct);
        if (person is null)
            return Result<Person>.Failure(
                "Customer profile not found. Complete personal data before ordering.");

        if (person.HomeAddress is null)
            return Result<Person>.Failure("Delivery address is missing.");

        return Result<Person>.Success(person);
    }

    private async Task<Result<Money>> CalculateShippingFeeAsync(
        Person person,
        CancellationToken ct)
    {
        var shippingRequest = new ShippingFeeCalculationRequest
        {
            FullName = person.Name.ToString(),
            PhoneNumber = person.PhoneNumber.Value,
            Street = person.HomeAddress!.Street,
            City = person.HomeAddress.City,
            LocationMapUrl = person.HomeAddress.LocationMapUrl!
        };

        var shippingResult = await _shippingFeeCalculator.CalculateAsync(shippingRequest, ct);
        if (shippingResult is null)
            return Result<Money>.Failure("Shipping fee calculation failed.");

        var shippingFee = new Money(shippingResult.Amount, shippingResult.Currency);
        return Result<Money>.Success(shippingFee);
    }

    private static List<OrderItem> BuildOrderItems(Cart cart)
    {
        return cart.Items
            .Select(cartItem => new OrderItem(
                productId: cartItem.ProductId,
                productVariantId: cartItem.ProductVariantId,
                sku: cartItem.Sku,
                productName: cartItem.ProductName,
                variantName: cartItem.VariantName,
                unitPrice: cartItem.UnitPrice,
                quantity: cartItem.Quantity))
            .ToList();
    }

    private async Task<Result> DecreaseStockAsync(
        List<OrderItem> orderItems,
        CancellationToken ct)
    {
        foreach (var item in orderItems)
        {
            var stockResult = await _stockService.DecreaseStockAsync(
                item.ProductId,
                item.ProductVariantId,
                item.Quantity,
                ct);

            if (!stockResult.Succeeded)
                return Result.Failure(
                    $"Insufficient stock for product {item.ProductName}.");
        }

        return Result.Success();
    }

    private static Order CreateOrder(
        Guid customerId,
        List<OrderItem> orderItems,
        Money shippingFee)
    {
        return Order.Place(customerId, orderItems, shippingFee);
    }

    private async Task PersistOrderAsync(
        Order order,
        Cart cart,
        CancellationToken ct)
    {
        await _orderRepository.AddAsync(order, ct);
        cart.Clear();
        await _cartRepository.UpdateAsync(cart, ct);
    }

    private void RecordOrderCreatedMetric()
    {
        _metrics.RecordCreated(customerTier: "standard");
    }

    private sealed record OrderPreparationResult(Order Order, Cart Cart);

    #endregion
}