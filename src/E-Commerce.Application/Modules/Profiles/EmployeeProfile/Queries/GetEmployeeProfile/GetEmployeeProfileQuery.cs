using E_Commerce.Application.Modules.Profiles.EmployeeProfile.Models;
using E_Commerce.Application.Shared.Models;
using MediatR;

namespace E_Commerce.Application.Modules.Profiles.EmployeeProfile.Queries.GetEmployeeProfile;

/// <summary>
/// Retrieves a denormalized employee profile with role and assignment
/// statistics (active/completed shipments, ratings, last activity)
/// from the read model.
/// </summary>
public sealed record GetEmployeeProfileQuery(Guid EmployeeId)
    : IRequest<Result<EmployeeProfileReadModel>>;