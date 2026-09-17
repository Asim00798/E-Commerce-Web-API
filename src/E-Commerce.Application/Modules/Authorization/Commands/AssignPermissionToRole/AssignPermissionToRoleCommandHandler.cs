using E_Commerce.Application.Shared.Models;
using E_Commerce.Application.Shared.Security.Authorization.Services;
using MediatR;

namespace E_Commerce.Application.Modules.Authorization.Commands.AssignPermissionToRole;

public sealed class AssignPermissionToRoleCommandHandler
    : IRequestHandler<AssignPermissionToRoleCommand, Result>
{
    private readonly IPermissionManagementService _permissionManagementService;

    public AssignPermissionToRoleCommandHandler(IPermissionManagementService permissionManagementService)
    {
        _permissionManagementService = permissionManagementService;
    }

    public async Task<Result> Handle(AssignPermissionToRoleCommand command, CancellationToken cancellationToken)
    {
        await _permissionManagementService.AssignPermissionToRoleAsync(
            command.RoleId,
            command.PermissionId,
            cancellationToken);

        return Result.Success();
    }
}