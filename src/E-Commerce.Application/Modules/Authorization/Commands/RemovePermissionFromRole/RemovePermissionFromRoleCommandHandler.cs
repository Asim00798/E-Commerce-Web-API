using E_Commerce.Application.Modules.Authorization.Abstractions;
using E_Commerce.Application.Shared.Models;
using MediatR;

namespace E_Commerce.Application.Modules.Authorization.Commands.RemovePermissionFromRole;

public sealed class RemovePermissionFromRoleCommandHandler
    : IRequestHandler<RemovePermissionFromRoleCommand, Result>
{
    private readonly IPermissionManagementService _permissionManagementService;

    public RemovePermissionFromRoleCommandHandler(IPermissionManagementService permissionManagementService)
    {
        _permissionManagementService = permissionManagementService;
    }

    public async Task<Result> Handle(RemovePermissionFromRoleCommand command, CancellationToken cancellationToken)
    {
        await _permissionManagementService.RemovePermissionFromRoleAsync(
            command.RoleId,
            command.PermissionId,
            cancellationToken);

        return Result.Success();
    }
}