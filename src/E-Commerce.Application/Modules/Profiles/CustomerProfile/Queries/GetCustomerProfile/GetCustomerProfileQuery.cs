using E_Commerce.Application.Modules.Profiles.CustomerProfile.Models;
using E_Commerce.Application.Shared.Models;
using MediatR;

namespace E_Commerce.Application.Modules.Profiles.CustomerProfile.Queries.GetCustomerProfile;

/// <summary>
/// Retrieves a denormalized customer profile with aggregated metrics
/// (orders, spend, ratings, wishlist, last activity) from the read model.
/// </summary>
public sealed record GetCustomerProfileQuery(Guid CustomerId)
    : IRequest<Result<CustomerProfileReadModel>>;