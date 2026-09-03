using E_Commerce.Application.Modules.Authorization.Abstractions;
using E_Commerce.Application.Shared.Models;
using MediatR;

namespace E_Commerce.Application.Modules.Authorization.Commands.AssignRoleToUser;

public sealed class AssignRoleToUserCommandHandler
    : IRequestHandler<AssignRoleToUserCommand, Result>
{
    private readonly IRoleManagementService _roleManagementService;

    public AssignRoleToUserCommandHandler(IRoleManagementService roleManagementService)
    {
        _roleManagementService = roleManagementService;
    }

    public async Task<Result> Handle(AssignRoleToUserCommand command, CancellationToken cancellationToken)
    {
        await _roleManagementService.AssignRoleToUserAsync(
            command.UserId,
            command.Role,
            cancellationToken);

        return Result.Success();
    }
}