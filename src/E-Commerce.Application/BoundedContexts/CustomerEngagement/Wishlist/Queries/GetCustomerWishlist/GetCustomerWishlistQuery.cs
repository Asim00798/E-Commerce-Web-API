using E_Commerce.Application.BoundedContexts.CustomerEngagement.Wishlist.DTOs;
using E_Commerce.Application.Shared.Models;
using E_Commerce.Application.Shared.Security.Authorization.Attributes;
using E_Commerce.Application.Shared.Security.Authorization.Permissions;
using MediatR;

namespace E_Commerce.Application.BoundedContexts.CustomerEngagement.Wishlist.Queries.GetCustomerWishlist;

[AuthorizePermission(CustomerEngagementPermissions.Read)]
public sealed record GetCustomerWishlistQuery : IRequest<Result<WishlistDto>>;