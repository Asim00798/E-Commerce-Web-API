namespace E_Commerce.Application.BoundedContexts.CustomerEngagement.Wishlist.DTOs;

public sealed class WishlistItemDto
{
    public Guid ProductId { get; init; }
    public DateTime AddedAtUtc { get; init; }
}