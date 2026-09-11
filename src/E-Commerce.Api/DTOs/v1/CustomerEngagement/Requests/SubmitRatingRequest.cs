namespace E_Commerce.Api.DTOs.v1.CustomerEngagement.Requests;

public sealed class SubmitRatingRequest
{
    public Guid ProductId { get; init; }
    public int StarRating { get; init; }
}