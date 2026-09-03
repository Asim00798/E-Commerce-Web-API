using E_Commerce.Application.Shared.Models;
using E_Commerce.Application.Shared.Security.Identity;
using E_Commerce.Domain.BoundedContexts.Core.CustomerEngagement.Repositories;
using E_Commerce.Domain.SharedKernel.PersistenceAbstractions;
using MediatR;

namespace E_Commerce.Application.BoundedContexts.CustomerEngagement.Wishlist.Commands.RemoveWishlistItem;

public sealed class RemoveWishlistItemCommandHandler
    : IRequestHandler<RemoveWishlistItemCommand, Result>
{
    private readonly IWishlistRepository _wishlistRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveWishlistItemCommandHandler(
        IWishlistRepository wishlistRepository,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork)
    {
        _wishlistRepository = wishlistRepository;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        RemoveWishlistItemCommand command,
        CancellationToken ct)
    {
        var wishlist = await _wishlistRepository.GetByCustomerIdAsync(
            _currentUser.UserId!.Value, ct);

        if (wishlist is null)
            return Result.Success(); // No wishlist = product not present; no-op success.

        wishlist.RemoveItem(command.ProductId);
        await _wishlistRepository.UpdateAsync(wishlist, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}