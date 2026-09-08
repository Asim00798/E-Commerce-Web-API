using E_Commerce.Application.BoundedContexts.CustomerEngagement.Ratings.DTOs;
using E_Commerce.Application.Shared.Models;
using E_Commerce.Application.Shared.Security.Identity;
using E_Commerce.Domain.BoundedContexts.Core.CustomerEngagement.AggregateRoots.Rating.Behaviors;
using E_Commerce.Domain.BoundedContexts.Core.CustomerEngagement.Repositories;
using MediatR;

namespace E_Commerce.Application.BoundedContexts.CustomerEngagement.Ratings.Queries.GetCustomerRatings;

public sealed class GetCustomerRatingsQueryHandler
    : IRequestHandler<GetCustomerRatingsQuery, Result<IReadOnlyList<RatingDto>>>
{
    private readonly IRatingRepository _ratingRepository;
    private readonly ICurrentUser _currentUser;

    public GetCustomerRatingsQueryHandler(
        IRatingRepository ratingRepository,
        ICurrentUser currentUser)
    {
        _ratingRepository = ratingRepository;
        _currentUser = currentUser;
    }

    public async Task<Result<IReadOnlyList<RatingDto>>> Handle(
        GetCustomerRatingsQuery query,
        CancellationToken ct)
    {
        var ratings = await _ratingRepository.GetByCustomerIdAsync(
            _currentUser.UserId!.Value, ct);

        var dtos = ratings.Select(MapToDto).ToList();

        return Result<IReadOnlyList<RatingDto>>.Success(dtos);
    }

    private static RatingDto MapToDto(Rating rating)
    {
        return new RatingDto
        {
            Id = rating.Id,
            CustomerId = rating.CustomerId,
            ProductId = rating.ProductId,
            StarRating = rating.StarRating.Value,
            CreatedAtUtc = rating.CreatedAtUtc,
            UpdatedAtUtc = rating.UpdatedAtUtc
        };
    }
}