using E_Commerce.Application.Modules.Identity.AccountManagement.Abstractions;
using E_Commerce.Application.Shared.Models;
using E_Commerce.Application.Shared.Security.Authorization.Services;
using E_Commerce.Application.Shared.Security.Identity;
using MediatR;

namespace E_Commerce.Application.Modules.Identity.AccountManagement.Commands.LockAccount;

public sealed class LockAccountCommandHandler : IRequestHandler<LockAccountCommand, Result>
{
    private readonly IAccountManagement _accountManagement;
    private readonly IPermissionService _authorizationService;
    private readonly ICurrentUser _currentUser;

    public LockAccountCommandHandler(
        IAccountManagement accountManagement,
        IPermissionService authorizationService,
        ICurrentUser currentUser)
    {
        _accountManagement = accountManagement;
        _authorizationService = authorizationService;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(LockAccountCommand command, CancellationToken ct)
    {
        var currentUserId = _currentUser.UserId;
        if (currentUserId is null)
            return Result.Failure("Unauthorized.");

        var authorized = await _authorizationService.HasPermissionAsync(
            currentUserId.Value,
            AccountPermissions.Lock);

        if (!authorized)
            return Result.Failure("Forbidden.");

        await _accountManagement.LockAsync(command.UserId, command.LockoutEnd, ct);
        return Result.Success();
    }
}