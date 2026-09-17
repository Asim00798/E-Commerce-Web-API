using E_Commerce.Application.Shared.Models;
using E_Commerce.Application.Shared.Security.Authorization.Services;
using MediatR;

namespace E_Commerce.Application.Modules.Authorization.Commands.CreatePermission;

public sealed class CreatePermissionCommandHandler
    : IRequestHandler<CreatePermissionCommand, Result<Guid>>
{
    private readonly IPermissionManagementService _permissionManagementService;

    public CreatePermissionCommandHandler(IPermissionManagementService permissionManagementService)
    {
        _permissionManagementService = permissionManagementService;
    }

    public async Task<Result<Guid>> Handle(CreatePermissionCommand command, CancellationToken cancellationToken)
    {
        var permissionId = await _permissionManagementService.CreatePermissionAsync(
            command.Name,
            command.Description,
            cancellationToken);

        return Result<Guid>.Success(permissionId);
    }
}