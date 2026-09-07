using E_Commerce.ReadModel.Features.CustomerProfile.Models;

namespace E_Commerce.ReadModel.Features.CustomerProfile.Queries;

public interface ICustomerProfileQueryService
{
    Task<CustomerProfileReadModel?> GetCustomerProfileAsync(
        Guid customerId,
        CancellationToken cancellationToken = default);
}