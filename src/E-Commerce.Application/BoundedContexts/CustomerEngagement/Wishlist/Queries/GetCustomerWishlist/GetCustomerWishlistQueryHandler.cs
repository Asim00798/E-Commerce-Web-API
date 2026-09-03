using E_Commerce.Application.BoundedContexts.CustomerEngagement.Wishlist.DTOs;
using E_Commerce.Application.Shared.Models;
using E_Commerce.Application.Shared.Security.Identity;
using E_Commerce.Domain.BoundedContexts.Core.CustomerEngagement.Repositories;
using MediatR;

namespace E_Commerce.Application.BoundedContexts.CustomerEngagement.Wishlist.Queries.GetCustomerWishlist;

public sealed class GetCustomerWishlistQueryHandler
    : IRequestHandler<GetCustomerWishlistQuery, Result<WishlistDto>>
{
    private readonly IWishlistRepository _wishlistRepository;
    private readonly ICurrentUser _currentUser;

    public GetCustomerWishlistQueryHandler(
        IWishlistRepository wishlistRepository,
        ICurrentUser currentUser)
    {
        _wishlistRepository = wishlistRepository;
        _currentUser = currentUser;
    }

    public async Task<Result<WishlistDto>> Handle(
        GetCustomerWishlistQuery query,
        CancellationToken ct)
    {
        var wishlist = await _wishlistRepository.GetByCustomerIdAsync(
            _currentUser.UserId!.Value, ct);

        if (wishlist is null)
        {
            return Result<WishlistDto>.Success(new WishlistDto
            {
                Id = Guid.Empty,
                CustomerId = _currentUser.UserId.Value,
                Items = new List<WishlistItemDto>()
            });
        }

        var dto = new WishlistDto
        {
            Id = wishlist.Id,
            CustomerId = wishlist.CustomerId,
            Items = wishlist.Items.Select(i => new WishlistItemDto
            {
                ProductId = i.ProductId,
                AddedAtUtc = i.AddedAtUtc
            }).ToList()
        };

        return Result<WishlistDto>.Success(dto);
    }
}