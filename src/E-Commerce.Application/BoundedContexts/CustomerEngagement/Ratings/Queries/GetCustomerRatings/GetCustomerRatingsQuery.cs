using E_Commerce.Application.BoundedContexts.CustomerEngagement.Ratings.DTOs;
using E_Commerce.Application.Shared.Models;
using E_Commerce.Application.Shared.Security.Authorization.Attributes;
using E_Commerce.Application.Shared.Security.Authorization.Permissions;
using MediatR;

namespace E_Commerce.Application.BoundedContexts.CustomerEngagement.Ratings.Queries.GetCustomerRatings;

[AuthorizePermission(CustomerEngagementPermissions.Read)]
public sealed record GetCustomerRatingsQuery : IRequest<Result<IReadOnlyList<RatingDto>>>;