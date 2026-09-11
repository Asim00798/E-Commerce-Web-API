namespace E_Commerce.Api.DTOs.v1.CustomerEngagement.Responses;

public sealed class WishlistItemResponse
{
    public Guid ProductId { get; init; }
    public DateTime AddedAtUtc { get; init; }
}