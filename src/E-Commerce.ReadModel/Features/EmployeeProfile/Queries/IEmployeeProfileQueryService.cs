using E_Commerce.ReadModel.Features.EmployeeProfile.Models;

namespace E_Commerce.ReadModel.Features.EmployeeProfile.Queries;

public interface IEmployeeProfileQueryService
{
    Task<EmployeeProfileReadModel?> GetEmployeeProfileAsync(
        Guid employeeId,
        CancellationToken cancellationToken = default);
}