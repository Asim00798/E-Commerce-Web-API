namespace E_Commerce.Application.BoundedContexts.CustomerEngagement.Wishlist.DTOs;

public sealed class WishlistDto
{
    public Guid Id { get; init; }
    public Guid CustomerId { get; init; }
    public IReadOnlyList<WishlistItemDto> Items { get; init; } = new List<WishlistItemDto>();
}