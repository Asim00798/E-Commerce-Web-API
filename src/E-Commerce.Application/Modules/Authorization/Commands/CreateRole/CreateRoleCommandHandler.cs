using E_Commerce.Application.Modules.Authorization.Abstractions;
using E_Commerce.Application.Shared.Models;
using MediatR;

namespace E_Commerce.Application.Modules.Authorization.Commands.CreateRole;

public sealed class CreateRoleCommandHandler
    : IRequestHandler<CreateRoleCommand, Result<Guid>>
{
    private readonly IRoleManagementService _roleManagementService;

    public CreateRoleCommandHandler(IRoleManagementService roleManagementService)
    {
        _roleManagementService = roleManagementService;
    }

    public async Task<Result<Guid>> Handle(CreateRoleCommand command, CancellationToken cancellationToken)
    {
        var roleId = await _roleManagementService.CreateRoleAsync(command.Name, cancellationToken);
        return Result<Guid>.Success(roleId);
    }
}