using E_Commerce.Application.Modules.Authorization.Abstractions;
using E_Commerce.Application.Shared.Models;
using MediatR;

namespace E_Commerce.Application.Modules.Authorization.Commands.RemoveRoleFromUser;

public sealed class RemoveRoleFromUserCommandHandler
    : IRequestHandler<RemoveRoleFromUserCommand, Result>
{
    private readonly IRoleManagementService _roleManagementService;

    public RemoveRoleFromUserCommandHandler(IRoleManagementService roleManagementService)
    {
        _roleManagementService = roleManagementService;
    }

    public async Task<Result> Handle(RemoveRoleFromUserCommand command, CancellationToken cancellationToken)
    {
        await _roleManagementService.RemoveRoleFromUserAsync(
            command.UserId,
            command.Role,
            cancellationToken);

        return Result.Success();
    }
}