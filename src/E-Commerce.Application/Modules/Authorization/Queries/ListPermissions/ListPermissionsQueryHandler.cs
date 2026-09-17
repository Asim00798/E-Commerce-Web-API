using E_Commerce.Application.Modules.Authorization.Dtos;
using E_Commerce.Application.Shared.Models;
using E_Commerce.Application.Shared.Security.Authorization.Services;
using MediatR;

namespace E_Commerce.Application.Modules.Authorization.Queries.ListPermissions;

public sealed class ListPermissionsQueryHandler
    : IRequestHandler<ListPermissionsQuery, Result<IReadOnlyList<PermissionDto>>>
{
    private readonly IPermissionManagementService _permissionManagementService;

    public ListPermissionsQueryHandler(IPermissionManagementService permissionManagementService)
    {
        _permissionManagementService = permissionManagementService;
    }

    public async Task<Result<IReadOnlyList<PermissionDto>>> Handle(ListPermissionsQuery query, CancellationToken cancellationToken)
    {
        var permissions = await _permissionManagementService.GetPermissionsAsync(cancellationToken);
        return Result<IReadOnlyList<PermissionDto>>.Success(permissions);
    }
}