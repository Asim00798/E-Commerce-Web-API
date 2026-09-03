using E_Commerce.Application.Modules.Authorization.Abstractions;
using E_Commerce.Application.Shared.Models;
using MediatR;

namespace E_Commerce.Application.Modules.Authorization.Commands.DeletePermission;

public sealed class DeletePermissionCommandHandler
    : IRequestHandler<DeletePermissionCommand, Result>
{
    private readonly IPermissionManagementService _permissionManagementService;

    public DeletePermissionCommandHandler(IPermissionManagementService permissionManagementService)
    {
        _permissionManagementService = permissionManagementService;
    }

    public async Task<Result> Handle(DeletePermissionCommand command, CancellationToken cancellationToken)
    {
        await _permissionManagementService.DeletePermissionAsync(command.PermissionId, cancellationToken);
        return Result.Success();
    }
}