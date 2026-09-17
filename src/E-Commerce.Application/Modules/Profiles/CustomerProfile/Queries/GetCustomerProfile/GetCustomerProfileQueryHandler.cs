using E_Commerce.Application.Modules.Profiles.CustomerProfile.Abstractions;
using E_Commerce.Application.Modules.Profiles.CustomerProfile.Models;
using E_Commerce.Application.Shared.Models;
using MediatR;

namespace E_Commerce.Application.Modules.Profiles.CustomerProfile.Queries.GetCustomerProfile;

public sealed class GetCustomerProfileQueryHandler
    : IRequestHandler<GetCustomerProfileQuery, Result<CustomerProfileReadModel>>
{
    private readonly ICustomerProfileQueryService _queryService;

    public GetCustomerProfileQueryHandler(ICustomerProfileQueryService queryService)
    {
        _queryService = queryService;
    }

    public async Task<Result<CustomerProfileReadModel>> Handle(
        GetCustomerProfileQuery query,
        CancellationToken ct)
    {
        var profile = await _queryService.GetCustomerProfileAsync(query.CustomerId, ct);

        if (profile is null)
            return Result<CustomerProfileReadModel>.Failure("Customer profile not found.");

        return Result<CustomerProfileReadModel>.Success(profile);
    }
}