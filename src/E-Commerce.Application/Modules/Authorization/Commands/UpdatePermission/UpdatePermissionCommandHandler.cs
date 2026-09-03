using E_Commerce.Application.Modules.Authorization.Abstractions;
using E_Commerce.Application.Shared.Models;
using MediatR;

namespace E_Commerce.Application.Modules.Authorization.Commands.UpdatePermission;

public sealed class UpdatePermissionCommandHandler
    : IRequestHandler<UpdatePermissionCommand, Result>
{
    private readonly IPermissionManagementService _permissionManagementService;

    public UpdatePermissionCommandHandler(IPermissionManagementService permissionManagementService)
    {
        _permissionManagementService = permissionManagementService;
    }

    public async Task<Result> Handle(UpdatePermissionCommand command, CancellationToken cancellationToken)
    {
        await _permissionManagementService.UpdatePermissionAsync(
            command.PermissionId,
            command.Name,
            command.Description,
            cancellationToken);

        return Result.Success();
    }
}