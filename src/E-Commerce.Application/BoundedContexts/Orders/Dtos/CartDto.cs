using Application.BoundedContexts.Ordering.Dtos;

namespace E_Commerce.Application.BoundedContexts.Orders.Dtos;

public sealed class CartDto
{
    public Guid Id { get; init; }
    public Guid CustomerId { get; init; }
    public IReadOnlyList<CartItemDto> Items { get; init; } = new List<CartItemDto>();
}