using E_Commerce.Application.Shared.Models;
using E_Commerce.Application.Shared.Security.Authorization.Services;
using MediatR;

namespace E_Commerce.Application.Modules.Authorization.Commands.DeleteRole;

public sealed class DeleteRoleCommandHandler
    : IRequestHandler<DeleteRoleCommand, Result>
{
    private readonly IRoleManagementService _roleManagementService;

    public DeleteRoleCommandHandler(IRoleManagementService roleManagementService)
    {
        _roleManagementService = roleManagementService;
    }

    public async Task<Result> Handle(DeleteRoleCommand command, CancellationToken cancellationToken)
    {
        await _roleManagementService.DeleteRoleAsync(command.RoleId, cancellationToken);
        return Result.Success();
    }
}