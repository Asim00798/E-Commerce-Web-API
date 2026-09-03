using E_Commerce.Application.BoundedContexts.CustomerEngagement.Ratings.DTOs;
using E_Commerce.Application.Shared.Models;
using E_Commerce.Application.Shared.Security.Authorization.Attributes;
using E_Commerce.Application.Shared.Security.Authorization.Permissions;
using MediatR;

namespace E_Commerce.Application.BoundedContexts.CustomerEngagement.Ratings.Queries.GetProductRatings;

[AuthorizePermission(CustomerEngagementPermissions.Read)]
public sealed record GetProductRatingsQuery(Guid ProductId)
    : IRequest<Result<ProductRatingsSummaryDto>>;