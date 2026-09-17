using E_Commerce.Application.Modules.Profiles.CustomerProfile.Models;

namespace E_Commerce.Application.Modules.Profiles.CustomerProfile.Abstractions;

public interface ICustomerProfileQueryService
{
    Task<CustomerProfileReadModel?> GetCustomerProfileAsync(
        Guid customerId,
        CancellationToken cancellationToken = default);
}