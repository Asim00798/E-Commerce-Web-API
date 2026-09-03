using E_Commerce.Application.Shared.Models;
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

    public PlaceOrderCommandHandler(
        ICartRepository cartRepository,
        IOrderRepository orderRepository,
        IPersonRepository personRepository,
        IStockService stockService,
        IShippingFeeCalculator shippingFeeCalculator,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork)
    {
        _cartRepository = cartRepository;
        _orderRepository = orderRepository;
        _personRepository = personRepository;
        _stockService = stockService;
        _shippingFeeCalculator = shippingFeeCalculator;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(
        PlaceOrderCommand request,
        CancellationToken ct)
    {
        var customerId = _currentUser.UserId!.Value;

        var cartResult = await LoadCartAsync(customerId, ct);
        if (!cartResult.Succeeded)
            return Result<Guid>.Failure(cartResult.Errors);

        var cart = cartResult.Data!;

        var personResult = await LoadPersonAsync(customerId, ct);
        if (!personResult.Succeeded)
            return Result<Guid>.Failure(personResult.Errors);

        var person = personResult.Data!;

        var shippingFeeResult = await CalculateShippingFeeAsync(person, ct);
        if (!shippingFeeResult.Succeeded)
            return Result<Guid>.Failure(shippingFeeResult.Errors);

        var shippingFee = shippingFeeResult.Data!;

        var orderItems = BuildOrderItems(cart);

        var stockResult = await DecreaseStockAsync(orderItems, ct);
        if (!stockResult.Succeeded)
            return Result<Guid>.Failure(stockResult.Errors);

        var order = Order.Place(customerId, orderItems, shippingFee);

        await _orderRepository.AddAsync(order, ct);
        cart.Clear();
        await _cartRepository.UpdateAsync(cart, ct);

        await _unitOfWork.SaveChangesAsync(ct);

        return Result<Guid>.Success(order.Id);
    }

    private async Task<Result<Cart>> LoadCartAsync(Guid customerId, CancellationToken ct)
    {
        var cart = await _cartRepository.GetByCustomerIdAsync(customerId, ct);
        if (cart is null || cart.Items.Count == 0)
            return Result<Cart>.Failure("Cart is empty or not found.");

        return Result<Cart>.Success(cart);
    }

    private async Task<Result<Person>> LoadPersonAsync(Guid customerId, CancellationToken ct)
    {
        var person = await _personRepository.GetByIdentityUserIdAsync(customerId, ct);
        if (person is null)
            return Result<Person>.Failure("Customer profile not found. Complete personal data before ordering.");

        if (person.HomeAddress is null)
            return Result<Person>.Failure("Delivery address is missing.");

        return Result<Person>.Success(person);
    }

    private async Task<Result<Money>> CalculateShippingFeeAsync(Person person, CancellationToken ct)
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

    private List<OrderItem> BuildOrderItems(Cart cart)
    {
        return cart.Items.Select(cartItem => new OrderItem(
            productId: cartItem.ProductId,
            productVariantId: cartItem.ProductVariantId,
            sku: cartItem.Sku,
            productName: cartItem.ProductName,
            variantName: cartItem.VariantName,
            unitPrice: cartItem.UnitPrice,
            quantity: cartItem.Quantity
        )).ToList();
    }

    private async Task<Result> DecreaseStockAsync(List<OrderItem> orderItems, CancellationToken ct)
    {
        foreach (var item in orderItems)
        {
            var stockResult = await _stockService.DecreaseStockAsync(
                item.ProductId,
                item.ProductVariantId,
                item.Quantity,
                ct);

            if (!stockResult.Succeeded)
                return Result.Failure($"Insufficient stock for product {item.ProductName}.");
        }

        return Result.Success();
    }
}