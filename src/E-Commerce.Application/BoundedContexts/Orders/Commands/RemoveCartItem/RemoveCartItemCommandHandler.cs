using E_Commerce.Application.Shared.Models;
using E_Commerce.Application.Shared.Security.Identity;
using E_Commerce.Domain.BoundedContexts.Core.Ordering.Repositories;
using E_Commerce.Domain.SharedKernel.PersistenceAbstractions;
using MediatR;

namespace E_Commerce.Application.BoundedContexts.Orders.Commands.RemoveCartItem;

public sealed class RemoveCartItemCommandHandler
    : IRequestHandler<RemoveCartItemCommand, Result>
{
    private readonly ICartRepository _cartRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveCartItemCommandHandler(
        ICartRepository cartRepository,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork)
    {
        _cartRepository = cartRepository;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        RemoveCartItemCommand request,
        CancellationToken ct)
    {
        var cart = await _cartRepository.GetByCustomerIdAsync(_currentUser.UserId!.Value, ct);
        if (cart is null)
            return Result.Failure("Cart not found.");

        cart.RemoveItem(request.ProductVariantId);

        await _cartRepository.UpdateAsync(cart, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}