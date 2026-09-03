using E_Commerce.Application.Modules.Authorization.Abstractions;
using E_Commerce.Application.Shared.Models;
using MediatR;

namespace E_Commerce.Application.Modules.Authorization.Commands.UpdateRole;

public sealed class UpdateRoleCommandHandler
    : IRequestHandler<UpdateRoleCommand, Result>
{
    private readonly IRoleManagementService _roleManagementService;

    public UpdateRoleCommandHandler(IRoleManagementService roleManagementService)
    {
        _roleManagementService = roleManagementService;
    }

    public async Task<Result> Handle(UpdateRoleCommand command, CancellationToken cancellationToken)
    {
        await _roleManagementService.UpdateRoleAsync(
            command.RoleId,
            command.Name,
            cancellationToken);

        return Result.Success();
    }
}