namespace E_Commerce.Api.DTOs.v1.CustomerEngagement.Responses;

public sealed class RatingResponse
{
    public Guid Id { get; init; }
    public Guid ProductId { get; init; }
    public int StarRating { get; init; }
    public DateTime CreatedAtUtc { get; init; }
    public DateTime? UpdatedAtUtc { get; init; }
}