using E_Commerce.Application.Modules.Profiles.EmployeeProfile.Models;

namespace E_Commerce.Application.Modules.Profiles.EmployeeProfile.Abstractions;

public interface IEmployeeProfileQueryService
{
    Task<EmployeeProfileReadModel?> GetEmployeeProfileAsync(
        Guid employeeId,
        CancellationToken cancellationToken = default);
}