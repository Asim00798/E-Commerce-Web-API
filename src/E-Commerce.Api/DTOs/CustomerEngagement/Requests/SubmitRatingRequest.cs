namespace E_Commerce.Api.DTOs.CustomerEngagement.Requests;

public sealed class SubmitRatingRequest
{
    public Guid ProductId { get; init; }
    public int StarRating { get; init; }
}