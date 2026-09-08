using E_Commerce.Application.Shared.Models;
using E_Commerce.Application.Shared.Security.Identity;
using E_Commerce.Domain.BoundedContexts.Core.Ordering.AggregateRoots.Cart.Behaviors;
using E_Commerce.Domain.BoundedContexts.Core.Ordering.AggregateRoots.Cart.Exceptions;
using E_Commerce.Domain.BoundedContexts.Core.Ordering.Repositories;
using E_Commerce.Domain.SharedKernel.PersistenceAbstractions;
using MediatR;

namespace E_Commerce.Application.BoundedContexts.Orders.Commands.UpdateCartItemQuantity;

public sealed class UpdateCartItemQuantityCommandHandler
    : IRequestHandler<UpdateCartItemQuantityCommand, Result>
{
    private readonly ICartRepository _cartRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCartItemQuantityCommandHandler(
        ICartRepository cartRepository,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork)
    {
        _cartRepository = cartRepository;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        UpdateCartItemQuantityCommand request,
        CancellationToken ct)
    {
        var customerId = GetCustomerId();
        if (customerId is null)
            return Result.Failure("Customer not found.");

        var cart = await LoadCartAsync(customerId.Value, ct);
        if (cart is null)
            return Result.Failure("Cart not found.");

        var updateResult = UpdateItemQuantity(cart, request);
        if (!updateResult.Succeeded)
            return updateResult;

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

    private static Result UpdateItemQuantity(
        Cart cart,
        UpdateCartItemQuantityCommand request)
    {
        try
        {
            cart.UpdateQuantity(
                request.ProductVariantId,
                request.NewQuantity);

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