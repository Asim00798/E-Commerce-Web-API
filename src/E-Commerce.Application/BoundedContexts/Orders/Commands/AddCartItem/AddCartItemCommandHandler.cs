using E_Commerce.Application.BoundedContexts.Orders.Models;
using E_Commerce.Application.Shared.Models;
using E_Commerce.Application.Shared.Security.Identity;
using E_Commerce.Domain.BoundedContexts.Core.Ordering.AggregateRoots.Cart.Behaviors;
using E_Commerce.Domain.BoundedContexts.Core.Ordering.AggregateRoots.Cart.Exceptions;
using E_Commerce.Domain.BoundedContexts.Core.Ordering.Repositories;
using E_Commerce.Domain.SharedKernel.PersistenceAbstractions;
using MediatR;
using Microsoft.Extensions.Options;

namespace E_Commerce.Application.BoundedContexts.Orders.Commands.AddCartItem;

public sealed class AddCartItemCommandHandler
    : IRequestHandler<AddCartItemCommand, Result>
{
    private readonly ICartRepository _cartRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _unitOfWork;
    private readonly OrderingOptions _orderingOptions;

    public AddCartItemCommandHandler(
        ICartRepository cartRepository,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork,
        IOptions<OrderingOptions> orderingOptions)
    {
        _cartRepository = cartRepository;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
        _orderingOptions = orderingOptions.Value;
    }

    public async Task<Result> Handle(
        AddCartItemCommand request,
        CancellationToken ct)
    {
        var customerId = GetCustomerId();
        if (customerId is null)
            return Result.Failure("Customer not found.");

        var cart = await LoadCartAsync(customerId.Value, ct);
        if (cart is null)
            return Result.Failure("Cart not found. Create a cart first.");

        var canAdd = EnsureCanAddItem(cart, request);
        if (!canAdd.Succeeded)
            return canAdd;

        var addResult = AddItemToCart(cart, request);
        if (!addResult.Succeeded)
            return addResult;

        await SaveCartAsync(cart, ct);

        return Result.Success();
    }

    #region Private Methods
    private Guid? GetCustomerId()
    {
        return _currentUser.UserId;
    }

    private async Task<Cart?> LoadCartAsync(
        Guid customerId,
        CancellationToken ct)
    {
        return await _cartRepository.GetByCustomerIdAsync(customerId, ct);
    }

    private Result EnsureCanAddItem(
        Cart cart,
        AddCartItemCommand request)
    {
        bool itemExists = cart.Items.Any(
            x => x.ProductVariantId == request.ProductVariantId);

        if (!itemExists && cart.Items.Count >= _orderingOptions.MaximumItemsPerCart)
        {
            return Result.Failure(
                $"Cannot add more than {_orderingOptions.MaximumItemsPerCart} different items to the cart.");
        }

        return Result.Success();
    }

    private static Result AddItemToCart(
        Cart cart,
        AddCartItemCommand request)
    {
        try
        {
            cart.AddItem(
                request.ProductId,
                request.ProductVariantId,
                request.Sku,
                request.ProductName,
                request.VariantName,
                request.UnitPrice,
                request.Quantity);

            return Result.Success();
        }
        catch (CartException ex)
        {
            return Result.Failure(ex.Message);
        }
    }

    private async Task SaveCartAsync(
        Cart cart,
        CancellationToken ct)
    {
        await _cartRepository.UpdateAsync(cart, ct);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    #endregion
}