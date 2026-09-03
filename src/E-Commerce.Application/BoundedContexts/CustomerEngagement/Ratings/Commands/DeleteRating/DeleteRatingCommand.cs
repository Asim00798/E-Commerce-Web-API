using E_Commerce.Application.Shared.Models;
using E_Commerce.Application.Shared.Security.Authorization.Attributes;
using E_Commerce.Application.Shared.Security.Authorization.Permissions;
using MediatR;

namespace E_Commerce.Application.BoundedContexts.CustomerEngagement.Ratings.Commands.DeleteRating;

[AuthorizePermission(CustomerEngagementPermissions.Rate)]
public sealed record DeleteRatingCommand(Guid RatingId) : IRequest<Result>;