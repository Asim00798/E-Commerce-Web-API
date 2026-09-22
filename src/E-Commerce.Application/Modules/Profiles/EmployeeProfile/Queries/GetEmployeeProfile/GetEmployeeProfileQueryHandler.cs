using E_Commerce.Application.Modules.Profiles.EmployeeProfile.Abstractions;
using E_Commerce.Application.Modules.Profiles.EmployeeProfile.Models;
using E_Commerce.Application.Shared.Models;
using MediatR;

namespace E_Commerce.Application.Modules.Profiles.EmployeeProfile.Queries.GetEmployeeProfile;

public sealed class GetEmployeeProfileQueryHandler
    : IRequestHandler<GetEmployeeProfileQuery, Result<EmployeeProfileReadModel>>
{
    private readonly IEmployeeProfileQueryService _queryService;

    public GetEmployeeProfileQueryHandler(IEmployeeProfileQueryService queryService)
    {
        _queryService = queryService;
    }

    public async Task<Result<EmployeeProfileReadModel>> Handle(
        GetEmployeeProfileQuery query,
        CancellationToken ct)
    {
        var profile = await _queryService.GetEmployeeProfileAsync(query.EmployeeId, ct);

        if (profile is null)
            return Result<EmployeeProfileReadModel>.Failure("Employee profile not found.");

        return Result<EmployeeProfileReadModel>.Success(profile);
    }
}