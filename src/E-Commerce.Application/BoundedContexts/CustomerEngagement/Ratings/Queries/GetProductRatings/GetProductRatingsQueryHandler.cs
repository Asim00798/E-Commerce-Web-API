using E_Commerce.Application.BoundedContexts.CustomerEngagement.Ratings.DTOs;
using E_Commerce.Application.Shared.Models;
using E_Commerce.Domain.BoundedContexts.Core.CustomerEngagement.Repositories;
using MediatR;

namespace E_Commerce.Application.BoundedContexts.CustomerEngagement.Ratings.Queries.GetProductRatings;

public sealed class GetProductRatingsQueryHandler
    : IRequestHandler<GetProductRatingsQuery, Result<ProductRatingsSummaryDto>>
{
    private readonly IRatingRepository _ratingRepository;

    public GetProductRatingsQueryHandler(IRatingRepository ratingRepository)
    {
        _ratingRepository = ratingRepository;
    }

    public async Task<Result<ProductRatingsSummaryDto>> Handle(
        GetProductRatingsQuery query,
        CancellationToken ct)
    {
        var summary = await _ratingRepository.GetProductRatingsSummaryAsync(
            query.ProductId, ct);

        if (summary is null)
        {
            return Result<ProductRatingsSummaryDto>.Success(new ProductRatingsSummaryDto
            {
                ProductId = query.ProductId,
                AverageRating = 0,
                TotalCount = 0,
                Distribution = new Dictionary<int, int>()
            });
        }

        var dto = new ProductRatingsSummaryDto
        {
            ProductId = query.ProductId,
            AverageRating = Math.Round(summary.AverageRating, 2),
            TotalCount = summary.TotalCount,
            Distribution = summary.Distribution.ToDictionary(k => k.Key, v => v.Value)
        };

        return Result<ProductRatingsSummaryDto>.Success(dto);
    }
}