using E_Commerce.Application.Modules.Authorization.Dtos;
using E_Commerce.Application.Shared.Models;
using E_Commerce.Application.Shared.Security.Authorization.Services;
using MediatR;

namespace E_Commerce.Application.Modules.Authorization.Queries.GetPermissionById;

public sealed class GetPermissionByIdQueryHandler
    : IRequestHandler<GetPermissionByIdQuery, Result<PermissionDto>>
{
    private readonly IPermissionManagementService _permissionManagementService;

    public GetPermissionByIdQueryHandler(IPermissionManagementService permissionManagementService)
    {
        _permissionManagementService = permissionManagementService;
    }

    public async Task<Result<PermissionDto>> Handle(GetPermissionByIdQuery query, CancellationToken cancellationToken)
    {
        var permission = await _permissionManagementService.GetPermissionByIdAsync(query.PermissionId, cancellationToken);
        if (permission is null)
            return Result<PermissionDto>.Failure("Permission not found.");

        return Result<PermissionDto>.Success(permission);
    }
}