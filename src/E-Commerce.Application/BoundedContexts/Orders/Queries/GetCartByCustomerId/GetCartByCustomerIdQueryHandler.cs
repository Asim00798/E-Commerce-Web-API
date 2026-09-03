using E_Commerce.Application.BoundedContexts.Orders.Dtos;
using E_Commerce.Application.Shared.Models;
using E_Commerce.Application.Shared.Security.Authorization.Permissions;
using E_Commerce.Application.Shared.Security.Identity;
using E_Commerce.Domain.BoundedContexts.Core.Ordering.Repositories;
using MediatR;

namespace E_Commerce.Application.BoundedContexts.Orders.Queries.GetCartByCustomerId;

public sealed class GetCartByCustomerIdQueryHandler
    : IRequestHandler<GetCartByCustomerIdQuery, Result<CartDto>>
{
    private readonly ICartRepository _cartRepository;
    private readonly ICurrentUser _currentUser;

    public GetCartByCustomerIdQueryHandler(
        ICartRepository cartRepository,
        ICurrentUser currentUser)
    {
        _cartRepository = cartRepository;
        _currentUser = currentUser;
    }

    public async Task<Result<CartDto>> Handle(
        GetCartByCustomerIdQuery query,
        CancellationToken ct)
    {
        // Resource authorization: customer can only access own cart
        if (_currentUser.UserId != query.CustomerId)
        {
            return Result<CartDto>.Failure("You are not authorized to access this cart.");
        }

        var cart = await _cartRepository.GetByCustomerIdAsync(query.CustomerId, ct);
        if (cart is null)
            return Result<CartDto>.Failure("Cart not found.");

        var cartDto = new CartDto
        {
            Id = cart.Id,
            CustomerId = cart.CustomerId,
            Items = cart.Items.Select(item => new CartItemDto
            {
                Id = item.Id,
                ProductId = item.ProductId,
                ProductVariantId = item.ProductVariantId,
                Sku = item.Sku,
                ProductName = item.ProductName,
                VariantName = item.VariantName,
                UnitPrice = item.UnitPrice.Amount,
                Currency = item.UnitPrice.Currency,
                Quantity = item.Quantity
            }).ToList()
        };

        return Result<CartDto>.Success(cartDto);
    }
}