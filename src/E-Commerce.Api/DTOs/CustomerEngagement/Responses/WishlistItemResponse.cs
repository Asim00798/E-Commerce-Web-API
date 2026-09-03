namespace E_Commerce.Api.DTOs.CustomerEngagement.Responses;

public sealed class WishlistItemResponse
{
    public Guid ProductId { get; init; }
    public DateTime AddedAtUtc { get; init; }
}