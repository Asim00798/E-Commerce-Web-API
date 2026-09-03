using E_Commerce.Application.Shared.Models;
using E_Commerce.Application.Shared.Security.Identity;
using WishlistAggregate = E_Commerce.Domain.BoundedContexts.Core.CustomerEngagement.AggregateRoots.Wishlist.Behaviors.Wishlist;
using E_Commerce.Domain.BoundedContexts.Core.CustomerEngagement.Repositories;
using E_Commerce.Domain.SharedKernel.PersistenceAbstractions;
using MediatR;

namespace E_Commerce.Application.BoundedContexts.CustomerEngagement.Wishlist.Commands.AddWishlistItem;

public sealed class AddWishlistItemCommandHandler
    : IRequestHandler<AddWishlistItemCommand, Result>
{
    private readonly IWishlistRepository _wishlistRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public AddWishlistItemCommandHandler(
        IWishlistRepository wishlistRepository,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork)
    {
        _wishlistRepository = wishlistRepository;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        AddWishlistItemCommand command,
        CancellationToken ct)
    {
        var customerId = _currentUser.UserId!.Value;

        WishlistAggregate? wishlist = await _wishlistRepository.GetByCustomerIdAsync(customerId, ct);

        if (wishlist is null)
        {
            wishlist = WishlistAggregate.Create(customerId);
            await _wishlistRepository.AddAsync(wishlist, ct);
        }

        wishlist.AddItem(command.ProductId);
        await _wishlistRepository.UpdateAsync(wishlist, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}