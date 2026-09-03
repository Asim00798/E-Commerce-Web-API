using E_Commerce.Application.Modules.Authorization.Abstractions;
using E_Commerce.Application.Modules.Authorization.Dtos;
using E_Commerce.Application.Shared.Models;
using MediatR;

namespace E_Commerce.Application.Modules.Authorization.Queries.ListPermissionsForRole;

public sealed class ListPermissionsForRoleQueryHandler
    : IRequestHandler<ListPermissionsForRoleQuery, Result<IReadOnlyList<PermissionDto>>>
{
    private readonly IPermissionManagementService _permissionManagementService;

    public ListPermissionsForRoleQueryHandler(IPermissionManagementService permissionManagementService)
    {
        _permissionManagementService = permissionManagementService;
    }

    public async Task<Result<IReadOnlyList<PermissionDto>>> Handle(ListPermissionsForRoleQuery query, CancellationToken cancellationToken)
    {
        var permissions = await _permissionManagementService.GetPermissionsForRoleAsync(query.RoleId, cancellationToken);
        return Result<IReadOnlyList<PermissionDto>>.Success(permissions);
    }
}