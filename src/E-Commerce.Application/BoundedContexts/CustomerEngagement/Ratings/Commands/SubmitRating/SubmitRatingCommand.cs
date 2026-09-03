using E_Commerce.Application.Shared.Models;
using E_Commerce.Application.Shared.Security.Authorization.Attributes;
using E_Commerce.Application.Shared.Security.Authorization.Permissions;
using MediatR;

namespace E_Commerce.Application.BoundedContexts.CustomerEngagement.Ratings.Commands.SubmitRating;

[AuthorizePermission(CustomerEngagementPermissions.Rate)]
public sealed record SubmitRatingCommand(
    Guid ProductId,
    int StarRating) : IRequest<Result<Guid>>;