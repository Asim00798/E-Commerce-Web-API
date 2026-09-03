using E_Commerce.Application.Shared.Models;
using E_Commerce.Application.Shared.Security.Authorization.Attributes;
using E_Commerce.Application.Shared.Security.Authorization.Permissions;
using MediatR;

namespace E_Commerce.Application.BoundedContexts.CustomerEngagement.Wishlist.Commands.RemoveWishlistItem;

[AuthorizePermission(CustomerEngagementPermissions.Wishlist)]
public sealed record RemoveWishlistItemCommand(Guid ProductId) : IRequest<Result>;