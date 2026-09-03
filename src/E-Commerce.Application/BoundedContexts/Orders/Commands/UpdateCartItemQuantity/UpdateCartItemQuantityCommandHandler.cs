using E_Commerce.Application.Shared.Models;
using E_Commerce.Application.Shared.Security.Identity;
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
        var cart = await _cartRepository.GetByCustomerIdAsync(_currentUser.UserId!.Value, ct);
        if (cart is null)
            return Result.Failure("Cart not found.");

        try
        {
            cart.UpdateQuantity(request.ProductVariantId, request.NewQuantity);
        }
        catch (CartException ex)
        {
            return Result.Failure(ex.Message);
        }

        await _cartRepository.UpdateAsync(cart, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}