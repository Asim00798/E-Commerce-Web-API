using E_Commerce.Application.Modules.Authorization.Abstractions;
using E_Commerce.Application.Modules.Authorization.Dtos;
using E_Commerce.Application.Shared.Models;
using MediatR;

namespace E_Commerce.Application.Modules.Authorization.Queries.ListRoles;

public sealed class ListRolesQueryHandler
    : IRequestHandler<ListRolesQuery, Result<IReadOnlyList<RoleDto>>>
{
    private readonly IRoleManagementService _roleManagementService;

    public ListRolesQueryHandler(IRoleManagementService roleManagementService)
    {
        _roleManagementService = roleManagementService;
    }

    public async Task<Result<IReadOnlyList<RoleDto>>> Handle(ListRolesQuery query, CancellationToken cancellationToken)
    {
        var roles = await _roleManagementService.GetRolesAsync(cancellationToken);
        return Result<IReadOnlyList<RoleDto>>.Success(roles);
    }
}