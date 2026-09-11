namespace E_Commerce.Api.DTOs.v1.CustomerEngagement.Responses;

public sealed class WishlistResponse
{
    public Guid Id { get; init; }
    public Guid CustomerId { get; init; }
    public IReadOnlyList<WishlistItemResponse> Items { get; init; } = new List<WishlistItemResponse>();
}