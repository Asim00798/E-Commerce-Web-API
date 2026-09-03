using E_Commerce.Application.BoundedContexts.CustomerEngagement.Ratings.DTOs;
using E_Commerce.Application.Shared.Models;
using E_Commerce.Application.Shared.Security.Identity;
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

        var dtos = ratings.Select(r => new RatingDto
        {
            Id = r.Id,
            CustomerId = r.CustomerId,
            ProductId = r.ProductId,
            StarRating = r.StarRating.Value,
            CreatedAtUtc = r.CreatedAtUtc,
            UpdatedAtUtc = r.UpdatedAtUtc
        }).ToList();

        return Result<IReadOnlyList<RatingDto>>.Success(dtos);
    }
}