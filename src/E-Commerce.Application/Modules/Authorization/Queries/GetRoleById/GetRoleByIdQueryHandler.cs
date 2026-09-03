using E_Commerce.Application.Modules.Authorization.Abstractions;
using E_Commerce.Application.Modules.Authorization.Dtos;
using E_Commerce.Application.Shared.Models;
using MediatR;

namespace E_Commerce.Application.Modules.Authorization.Queries.GetRoleById;

public sealed class GetRoleByIdQueryHandler
    : IRequestHandler<GetRoleByIdQuery, Result<RoleDto>>
{
    private readonly IRoleManagementService _roleManagementService;

    public GetRoleByIdQueryHandler(IRoleManagementService roleManagementService)
    {
        _roleManagementService = roleManagementService;
    }

    public async Task<Result<RoleDto>> Handle(GetRoleByIdQuery query, CancellationToken cancellationToken)
    {
        var role = await _roleManagementService.GetRoleByIdAsync(query.RoleId, cancellationToken);
        if (role is null)
            return Result<RoleDto>.Failure("Role not found.");

        return Result<RoleDto>.Success(role);
    }
}